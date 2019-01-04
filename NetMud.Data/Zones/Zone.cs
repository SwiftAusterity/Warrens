using NetMud.CentralControl;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Inanimates;
using NetMud.Data.Tiles;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Gaia.Geographical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Zones
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : EntityPartial, IZone
    {
        #region Template and Framework Values
        public bool IsPlayer()
        {
            return false;
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IZoneTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IZoneTemplate), TemplateId));
        }

        /// <summary>
        /// The entrance coordinates if someone ends up in this zone without entrance coordinates
        /// </summary>
        public Coordinate BaseCoordinates { get; set; }

        /// <summary>
        /// Paths out of this zone
        /// </summary>
        public HashSet<IPathway> Pathways { get; set; }
        #endregion

        /// <summary>
        /// Base humidity for this
        /// </summary>
        public int Humidity { get; set; }

        /// <summary>
        /// Base temperature for this
        /// </summary>
        public int Temperature { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public IMap _map { get; set; }

        public IMap Map
        {
            get
            {
                if (_map == null)
                {
                    PopulateMap();
                }

                return _map;
            }
            private set
            {
                _map = value;
            }
        }

        /// <summary>
        /// Clouds, basically
        /// </summary>
        public IEnumerable<IWeatherEvent> WeatherEvents { get; set; }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            WeatherEvents = Enumerable.Empty<IWeatherEvent>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Zone(IZoneTemplate zone)
        {
            TemplateId = zone.Id;
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            WeatherEvents = Enumerable.Empty<IWeatherEvent>();

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Broadcast an event to the entire zone
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the sender</param>
        /// <param name="subject">the subject</param>
        /// <param name="target">the target</param>
        public void BroadcastEvent(string message, IEntity sender = null, IEntity subject = null, IEntity target = null)
        {
            MessageCluster mc = new MessageCluster
            {
                ToOrigin = new List<IMessage>() { new Message(message) }
            };

            BroadcastEvent(mc, sender, subject, target);
        }

        private void BroadcastEvent(IMessageCluster message, IEntity sender = null, IEntity subject = null, IEntity target = null)
        {
            message.ExecuteMessaging(sender, subject, target, this, null);
        }

        /// <summary>
        /// Get the live world associated with this live zone
        /// </summary>
        /// <returns>The world</returns>
        public IGaia GetWorld()
        {
            IGaiaTemplate gaiaData = Template<IZoneTemplate>().World;

            if (gaiaData != null)
            {
                return gaiaData.GetLiveInstance();
            }

            return null;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            IZoneTemplate zD = Template<IZoneTemplate>();
            bool canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            List<ICelestial> returnList = new List<ICelestial>();

            //if (!canSeeSky)
            //  return returnList;

            IGaia world = GetWorld();
            IEnumerable<ICelestialPosition> celestials = world.CelestialPositions;

            if (celestials.Count() > 0)
            {
                //TODO: Add cloud cover stuff
                float rotationalPosition = world.PlanetaryRotation;
                float orbitalPosition = world.OrbitalPosition;
                float currentBrightness = GetCurrentLuminosity();

                foreach (ICelestialPosition celestial in celestials)
                {
                    float celestialLumins = celestial.CelestialObject.Luminosity * AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.CelestialObject, celestial.Position, rotationalPosition, orbitalPosition
                                                                                                   , zD.Hemisphere, world.Template<IGaiaTemplate>().RotationalAngle);

                    //how washed out is this thing compared to how bright the room is
                    if (celestialLumins / currentBrightness > 0.01)
                    {
                        returnList.Add(celestial.CelestialObject);
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Get the current forecast for this zone
        /// </summary>
        /// <returns>Bunch of stuff</returns>
        public Tuple<PrecipitationAmount, PrecipitationType, HashSet<WeatherType>> CurrentForecast()
        {
            PrecipitationAmount pAmount = PrecipitationAmount.Clear;
            PrecipitationType pType = PrecipitationType.Clear;
            HashSet<WeatherType> wTypes = new HashSet<WeatherType>();

            float totalRainVolume = WeatherEvents.Where(wev => wev.Type != WeatherEventType.Tectonic).Sum(wev => wev.PrecipitationAmount);
            float totalStrength = WeatherEvents.Where(wev => wev.Type != WeatherEventType.Tectonic).Sum(wev => wev.Strength);

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Tectonic))
            {
                wTypes.Add(WeatherType.Earthquake);
            }

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Cyclone))
            {
                wTypes.Add(WeatherType.Tornado);
            }

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Typhoon))
            {
                wTypes.Add(WeatherType.Hurricane);
            }

            if (!wTypes.Contains(WeatherType.Tornado) && !wTypes.Contains(WeatherType.Hurricane))
            {
                if (totalStrength > 50)
                {
                    wTypes.Add(WeatherType.Storming);
                }
                else if (totalStrength > 10)
                {
                    wTypes.Add(WeatherType.Windy);
                }
            }

            if (totalRainVolume > 0)
            {
                wTypes.Add(WeatherType.Precipitation);

                if (totalRainVolume < 25)
                {
                    pAmount = PrecipitationAmount.Drizzle;
                }
                else if (totalRainVolume < 50)
                {
                    pAmount = PrecipitationAmount.Steady;
                }
                else if (totalRainVolume < 75)
                {
                    pAmount = PrecipitationAmount.Downpour;
                }
                else
                {
                    pAmount = PrecipitationAmount.Torrential;
                }

                int effTemp = EffectiveTemperature();

                if (effTemp > 5)
                {
                    pType = PrecipitationType.Rain;
                }
                else if (effTemp > -5)
                {
                    pType = PrecipitationType.Snow;
                }
                else
                {
                    pType = PrecipitationType.Freezing;
                }

                if (pType == PrecipitationType.Snow)
                {
                    if (totalStrength <= 10)
                    {
                        pType = PrecipitationType.Sleet;
                    }
                    else if (totalStrength >= 50)
                    {
                        pType = PrecipitationType.Hail;
                    }
                }
            }

            if (pAmount == PrecipitationAmount.Clear && pType == PrecipitationType.Clear)
            {
                wTypes.Add(WeatherType.Clear);
            }

            return new Tuple<PrecipitationAmount, PrecipitationType, HashSet<WeatherType>>(pAmount, pType, wTypes);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            IZoneTemplate zD = Template<IZoneTemplate>();
            float lumins = 0;
            bool canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            //TODO: Add cloud cover. Commented out for testing purposes ATM
            //if (canSeeSky)
            //{
            IGaia world = GetWorld();
            if (world != null)
            {
                IEnumerable<ICelestialPosition> celestials = world.CelestialPositions;
                float rotationalPosition = world.PlanetaryRotation;
                float orbitalPosition = world.OrbitalPosition;

                foreach (ICelestialPosition celestial in celestials)
                {
                    float celestialAffectModifier = AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.CelestialObject, celestial.Position, rotationalPosition, orbitalPosition
                                                                                                        , zD.Hemisphere, world.Template<IGaiaTemplate>().RotationalAngle);

                    lumins += celestial.CelestialObject.Luminosity * celestialAffectModifier;
                }
            }
            //}

            return lumins;
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        /// <param name="spawnTo">Where this will go</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IZoneTemplate bS = Template<IZoneTemplate>() ?? throw new InvalidOperationException("Missing backing data store on zone spawn event.");

            Keywords = bS.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            if (spawnTo?.CurrentZone == null)
            {
                spawnTo = new GlobalPosition(this);
            }

            Qualities = bS.Qualities;
            DecayEvents = bS.DecayEvents;
            Interactions = bS.Interactions;

            BaseCoordinates = bS.BaseCoordinates;
            Pathways = bS.Pathways;

            WeatherEvents = Enumerable.Empty<IWeatherEvent>();

            CurrentLocation = spawnTo;

            PopulateMap();

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IZone me = LiveCache.Get<IZone>(TemplateId, typeof(ZoneTemplate));

            //Isn't in the world currently
            if (me == default(IZone))
            {
                SpawnNewInWorld();
            }
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = me.TemplateId;
                Keywords = me.Keywords;
                CurrentLocation = new GlobalPosition(this);

                Qualities = me.Qualities;
                DecayEvents = me.DecayEvents;
                Interactions = me.Interactions;
                PopulateMap();
            }
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentContainer != null)
            {
                error = CurrentLocation.CurrentContainer.MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentContainer != null)
                {
                    error = newPosition.CurrentContainer.MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    //Check for intruders
                    newPosition = IntruderSlide(newPosition);

                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;
                }
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }

        public IZone GetLiveInstance()
        {
            return this;
        }

        public IEnumerable<IPathway> GetPathways(bool inward = false)
        {
            return Template<IZoneTemplate>().Pathways;
        }

        /// <summary>
        /// Get one of the pathways in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IPathway GetPathway(long x, long y)
        {
            return GetPathways().FirstOrDefault(path => path.OriginCoordinates.X == x && path.OriginCoordinates.Y == y);
        }

        /// <summary>
        /// Get any entities inside this
        /// </summary>
        /// <returns>Entity list</returns>
        public IEnumerable<IEntity> GetContainedEntities()
        {
            List<IEntity> returnList = new List<IEntity>();

            returnList.AddRange(LiveCache.GetAll<IPlayer>().Where(player => player.CurrentLocation.CurrentZone == this));
            returnList.AddRange(LiveCache.GetAll<INonPlayerCharacter>().Where(player => player.CurrentLocation.CurrentZone == this));

            return returnList;
        }

        public int EffectiveHumidity()
        {
            return Humidity;
        }

        public int EffectiveTemperature()
        {
            return Temperature;
        }

        public bool IsOutside()
        {
            return GeographicalUtilities.IsOutside(GetBiome());
        }

        public Biome GetBiome()
        {
            return Template<IZoneTemplate>().BaseBiome;
        }

        private void PopulateMap()
        {
            IZoneTemplate dt = Template<IZoneTemplate>();
            long[,] coordinatePlane = dt.Map != null ? dt.Map.CoordinateTilePlane : new long[100, 100];
            short maxX = (short)coordinatePlane.GetLength(0);
            short maxY = (short)coordinatePlane.GetLength(1);

            ITile[,] tileMap = new ITile[maxX, maxY];

            short x = 0, y = 0;
            for (x = 0; x < maxX; x++)
            {
                for (y = 0; y < maxY; y++)
                {
                    ITileTemplate dtTile = TemplateCache.Get<ITileTemplate>(coordinatePlane[x, y]);
                    tileMap[x, y] = new Tile(this, dtTile, new Coordinate(x, y));
                }
            }

            _map = new Map(tileMap, false);

            if (dt.Map != null)
            {
                //Run thru the spawns
                foreach (var itemSpawn in dt.Map.ItemSpawns)
                {
                    GlobalPosition spawnTo = new GlobalPosition(this) { CurrentCoordinates = itemSpawn.Placement };
                    Inanimate newEntity = new Inanimate(TemplateCache.Get<IInanimateTemplate>(itemSpawn.ItemId), spawnTo);
                }

                foreach (var npcSpawn in dt.Map.NPCSpawns)
                {
                    GlobalPosition spawnTo = new GlobalPosition(this) { CurrentCoordinates = npcSpawn.Placement };
                    INonPlayerCharacter newEntity = new NPCs.NonPlayerCharacter(TemplateCache.Get<INonPlayerCharacterTemplate>(npcSpawn.NPCId), spawnTo);
                }
            }
        }

        #region Processes
        internal override void KickoffProcesses()
        {
            //Start decay eventing for this zone
            base.KickoffProcesses();

            Processor.StartSubscriptionLoop("Decay_Tiles", () => ProcessDecayStart_Tiles(), 24 * 60, false);
        }

        private bool ProcessDecayStart_Tiles()
        {
            //We check once per day to see if any dormant decay events need to start
            foreach (ITile tile in Map.CoordinateTilePlane)
            {
                tile.ProcessDecayEvents();
            }

            return Save();
        }
        #endregion


        /// <summary>
        /// Returns whether or not the Actor has the right to alter things in this zone
        /// </summary>
        /// <param name="actor">the person acting</param>
        /// <returns>if they have the rights</returns>
        public bool HasOwnershipRights(IActor actor)
        {
            if (actor.IsPlayer())
            {
                var playerObject = (IPlayer)actor;

                if (playerObject.GamePermissionsRank >= StaffRank.Builder)
                {
                    return true;
                }

                if (CreatorRank == StaffRank.Player && Creator.GlobalIdentityHandle.Equals(playerObject.AccountHandle))
                {
                    return true;
                }

                //TODO: Co-op rules
            }

            return false;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Zone
            {
                Qualities = Qualities,
                TemplateId = TemplateId,
                Humidity = Humidity,
                Temperature = Temperature,
                Map = Map
            };
        }
    }
}
