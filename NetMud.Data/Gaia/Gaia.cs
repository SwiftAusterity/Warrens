using NetMud.CentralControl;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Zone;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    [Serializable]
    public class Gaia : EntityPartial, IGaia
    {
        #region Template and Framework Values
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IGaiaTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IGaiaTemplate), TemplateId));
        }

        /// <summary>
        /// The angle at which this world rotates in space. Irrelevant for fixed objects.
        /// </summary>
        [Range(0, 359, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Rotational Angle", Description = "The angle at which this world rotates in space. Irrelevant for fixed objects.")]
        [DataType(DataType.Text)]
        public float RotationalAngle { get; set; }
        #endregion

        /// <summary>
        /// The current time of day (and month and year)
        /// </summary>
        [UIHint("TimeOfDay")]
        public ITimeOfDay CurrentTimeOfDay { get; set; }

        /// <summary>
        /// Where the planet is rotationally
        /// </summary>
        [Range(0, 359, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Planetary Rotation", Description = "Where the planet is in its rotational movement right now.")]
        [DataType(DataType.Text)]
        public float PlanetaryRotation { get; set; }

        /// <summary>
        /// Where the planet is in its orbit
        /// </summary>
        [Display(Name = "Orbital Position", Description = "Where the planet is in its orbit.")]
        [DataType(DataType.Text)]
        public float OrbitalPosition { get; set; }

        /// <summary>
        /// Collection of weather patterns for this world
        /// </summary>
        [UIHint("MeterologicalFrontCollection")]
        public HashSet<MeterologicalFront> MeterologicalFronts { get; set; }

        /// <summary>
        /// Economic controller for this world
        /// </summary>
        [UIHint("Economy")]
        public IEconomy Macroeconomy { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        [UIHint("CelestialPositionCollection")]
        public HashSet<ICelestialPosition> CelestialPositions { get; set; }

        public Gaia()
        {
            Qualities = new HashSet<IQuality>();
            MeterologicalFronts = new HashSet<MeterologicalFront>();
            Macroeconomy = new Economy();
            CelestialPositions = new HashSet<ICelestialPosition>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Gaia(IGaiaTemplate world)
        {
            TemplateId = world.Id;
            Qualities = new HashSet<IQuality>();
            MeterologicalFronts = new HashSet<MeterologicalFront>();
            Macroeconomy = new Economy();
            CelestialPositions = new HashSet<ICelestialPosition>();

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
            foreach (IZone zone in GetZones())
            {
                zone.BroadcastEvent(message, sender, subject, target);
            }
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            return GetZones().Sum(z => z.GetCurrentLuminosity());
        }

        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IGaia me = LiveCache.Get<IGaia>(TemplateId, typeof(GaiaTemplate));

            //Isn't in the world currently
            if (me == default(IGaia))
            {
                SpawnNewInWorld();
            }
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = me.TemplateId;
                Keywords = me.Keywords;
                CurrentLocation = null;
                CurrentTimeOfDay = me.CurrentTimeOfDay;
                MeterologicalFronts = me.MeterologicalFronts;
                Macroeconomy = me.Macroeconomy;
                CelestialPositions = me.CelestialPositions;
                RotationalAngle = me.RotationalAngle;

                Qualities = me.Qualities;

                CurrentLocation = null;
                KickoffProcesses();
            }
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentLocation() != null)
            {
                error = CurrentLocation.CurrentLocation().MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentLocation() != null)
                {
                    error = newPosition.CurrentLocation().MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
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

        public IGaia GetLiveInstance()
        {
            return this;
        }

        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(1, 1, 1);
        }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        public IEnumerable<IZone> GetZones()
        {
            return LiveCache.GetAll<IZone>().Where(zone => zone.GetWorld().Equals(this));
        }

        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(null);
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IGaiaTemplate bS = Template<IGaiaTemplate>() ?? throw new InvalidOperationException("Missing backing data store on gaia spawn event.");

            Keywords = bS.Keywords;

            if (CelestialPositions == null || CelestialPositions.Count() == 0)
            {
                HashSet<ICelestialPosition> celestials = new HashSet<ICelestialPosition>();

                foreach (ICelestial body in bS.CelestialBodies)
                {
                    celestials.Add(new CelestialPosition(body, 0));
                }

                CelestialPositions = celestials;
            }

            RotationalAngle = bS.RotationalAngle;
            Qualities = bS.Qualities;

            //gotta spawn 2 per hemisphere
            if (MeterologicalFronts == null || MeterologicalFronts.Count() == 0)
            {
                MeterologicalFronts = AddFronts();
            }

            CurrentTimeOfDay = new TimeOfDay(bS.ChronologicalSystem);

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            Macroeconomy = new Economy(bS);

            UpsertToLiveWorldCache(true);

            CurrentLocation = new GlobalPosition(null, null, null);
            KickoffProcesses();

            UpsertToLiveWorldCache(true);

            Save();
        }

        private HashSet<MeterologicalFront> AddFronts()
        {
            Random rander = new Random();

            PressureSystem neLow = new PressureSystem()
            {
                Direction = HemispherePlacement.NorthEast,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(870, 980),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem neHigh = new PressureSystem()
            {
                Direction = HemispherePlacement.NorthEast,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(980, 1050),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem nwLow = new PressureSystem()
            {
                Direction = HemispherePlacement.NorthWest,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(870, 980),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem nwHigh = new PressureSystem()
            {
                Direction = HemispherePlacement.NorthWest,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(980, 1050),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem seLow = new PressureSystem()
            {
                Direction = HemispherePlacement.SouthEast,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(870, 980),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem seHigh = new PressureSystem()
            {
                Direction = HemispherePlacement.SouthEast,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(980, 1050),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem swLow = new PressureSystem()
            {
                Direction = HemispherePlacement.SouthWest,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(870, 980),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            PressureSystem swHigh = new PressureSystem()
            {
                Direction = HemispherePlacement.SouthWest,
                Angle = rander.Next(1, 45),
                Pressure = rander.Next(980, 1050),
                Size = rander.Next(31854, 95562),
                Speed = rander.Next(91014, 136521),
                Strength = rander.Next(1, 100)
            };

            return new HashSet<MeterologicalFront>
            {
                new MeterologicalFront(neLow, 0),
                new MeterologicalFront(neHigh, 0),
                new MeterologicalFront(nwLow, 0),
                new MeterologicalFront(nwHigh, 0),
                new MeterologicalFront(seLow, 0),
                new MeterologicalFront(seHigh, 0),
                new MeterologicalFront(swLow, 0),
                new MeterologicalFront(swHigh, 0)
            };
        }

        public override void KickoffProcesses()
        {
            Processor.StartSubscriptionLoop("Time", () => AdvanceTime(), 5 * 60, false);
            Processor.StartSubscriptionLoop("MacroEconomics", () => AdvanceEconomy(), 15 * 60, false);
            Processor.StartSubscriptionLoop("CelestialBodies", () => AdvanceCelestials(), 5 * 60, false);
            Processor.StartSubscriptionLoop("PressureSystems", () => AdvanceWeather(), 5 * 60, false);
        }

        private bool AdvanceEconomy()
        {
            foreach (IEconomicTrend trend in Macroeconomy.Trends)
            {
                if (trend.Trend < 1)
                {
                    trend.Adjustment += (decimal)0.1;
                    trend.Trend += 1;
                }
                else if (trend.Trend > 1)
                {
                    trend.Adjustment -= (decimal)0.1;
                    trend.Trend -= 1;
                }
            }

            foreach (IEconomicBasis basis in Macroeconomy.Bases)
            {
                if (basis.Trend < 1)
                {
                    basis.Adjustment += (decimal)0.1;
                    basis.Trend += 1;
                }
                else if (basis.Trend > 1)
                {
                    basis.Adjustment -= (decimal)0.1;
                    basis.Trend -= 1;
                }
            }

            Save();

            return true;
        }

        private bool AdvanceTime()
        {
            IChronology chronoSystem = Template<IGaiaTemplate>().ChronologicalSystem;
            if (CurrentTimeOfDay.BaseChronology == null)
            {
                CurrentTimeOfDay.BaseChronology = chronoSystem;
            }

            CurrentTimeOfDay.AdvanceByHour();

            if (CelestialPositions.Any(cp => cp.CelestialObject.OrientationType == CelestialOrientation.SolarBody))
            {
                int rotationalChange = 360 / chronoSystem.HoursPerDay;
                PlanetaryRotation += rotationalChange;

                int maxOrbit = chronoSystem.Months.Count() * chronoSystem.DaysPerMonth * chronoSystem.HoursPerDay;

                int orbitalChange = 1 / maxOrbit;
                OrbitalPosition += orbitalChange;

                if (OrbitalPosition >= maxOrbit)
                {
                    OrbitalPosition -= maxOrbit;
                }
            }

            Save();

            return true;
        }

        private bool AdvanceWeather()
        {
            Random rander = new Random();
            HashSet<MeterologicalFront> newFronts = new HashSet<MeterologicalFront>();
            foreach (MeterologicalFront front in MeterologicalFronts)
            {
                //TODO: Fix my bullshit math and switch weather event cloud types as they shift and fix altitude
                IPressureSystem frontFront = front.Event;
                float originalPressure = frontFront.Pressure;

                //Calculate strength and pressure changes
                IEnumerable<IZone> myZones = LiveCache.GetAll<IZone>().Where(z => z.IsOutside() && z.Hemisphere == frontFront.Direction);

                if (myZones.Count() > 0)
                {
                    double zoneInfluenceUp = myZones.Average(z => z.EffectiveHumidity());
                    double zoneInfluenceDown = myZones.Average(z => z.EffectiveTemperature());

                    frontFront.Pressure += DataUtility.TryConvert<float>(zoneInfluenceUp - (zoneInfluenceDown * 45));

                    int frontVariance = frontFront.Pressure > originalPressure ? -1 : 1;
                    frontFront.Strength = Math.Max(1, Math.Min(100, frontFront.Strength + frontVariance));

                    //Alter zones
                    foreach (IZone zone in myZones)
                    {
                        float zoneVariance = zone.WeatherEvents.Sum(wEvent => (wEvent.Strength + wEvent.Drain) / (wEvent.Altitude / 10000));

                        List<IWeatherEvent> zoneEventList = new List<IWeatherEvent>();
                        foreach (IWeatherEvent cloud in zone.WeatherEvents)
                        {
                            cloud.Drain += zoneVariance;
                            cloud.Coverage += cloud.Drain;
                            cloud.PrecipitationAmount += cloud.Drain;
                            cloud.Strength -= cloud.Drain;

                            if (cloud.Strength > 0)
                            {
                                zoneEventList.Add(cloud);
                            }
                        }

                        //Spawn new weather event?
                        if (zoneVariance < 10)
                        {
                            zoneEventList.Add(new WeatherEvent()
                            {
                                Altitude = rander.Next(7500, 50000),
                                Coverage = rander.Next(1, 35),
                                Drain = 0,
                                PrecipitationAmount = 0,
                                Strength = rander.Next(100, 1000),
                                Type = WeatherEventType.Altocumulus
                            });

                            zone.WeatherEvents = zoneEventList;

                            BroadcastEvent("Clouds begin to darken overhead.");
                        }

                        zone.Humidity += DataUtility.TryConvert<int>(Math.Max(870, Math.Min(1080, frontVariance * zoneVariance * 45)));
                        zone.Temperature += DataUtility.TryConvert<int>(Math.Max(-50, Math.Min(50, frontVariance * zoneVariance)));

                        zone.Save();
                    }
                }

                //Move it
                float newPosition = front.Position + frontFront.Speed;

                //TODO: Not hardcode this, but this is HMR not a normal mud
                int hemisphericLength = 3185495;

                //if this goes over it wants to move to a new hemisphere
                if (newPosition > hemisphericLength)
                {
                    newPosition = Math.Abs(hemisphericLength - (newPosition - hemisphericLength));
                    switch (frontFront.Direction)
                    {
                        case HemispherePlacement.NorthEast:
                            frontFront.Direction = HemispherePlacement.SouthWest;
                            break;
                        case HemispherePlacement.NorthWest:
                            frontFront.Direction = HemispherePlacement.SouthEast;
                            break;
                        case HemispherePlacement.SouthEast:
                            frontFront.Direction = HemispherePlacement.NorthWest;
                            break;
                        case HemispherePlacement.SouthWest:
                            frontFront.Direction = HemispherePlacement.NorthEast;
                            break;
                    }
                }

                newFronts.Add(new MeterologicalFront(frontFront, newPosition));
            }

            MeterologicalFronts = newFronts;
            Save();

            return true;

        }

        private bool AdvanceCelestials()
        {
            HashSet<ICelestialPosition> newCelestials = new HashSet<ICelestialPosition>();
            foreach (ICelestialPosition celestial in CelestialPositions)
            {
                if (celestial.CelestialObject.OrientationType == CelestialOrientation.SolarBody || celestial.CelestialObject.OrientationType == CelestialOrientation.ExtraSolar)
                {
                    newCelestials.Add(celestial);
                    continue;
                }

                float newPosition = celestial.Position + celestial.CelestialObject.Velocity;

                int orbitalRadius = (celestial.CelestialObject.Apogee + celestial.CelestialObject.Perigree) / 2;
                float fullOrbitDistance = (float)Math.PI * (float)Math.Pow(orbitalRadius, 2);

                //There are 
                if (newPosition > fullOrbitDistance)
                {
                    newPosition = Math.Abs(fullOrbitDistance - (fullOrbitDistance - newPosition));
                }

                newCelestials.Add(new CelestialPosition(celestial.CelestialObject, newPosition));
            }

            CelestialPositions = newCelestials;
            Save();

            return true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Gaia
            {
                Qualities = Qualities,
                TemplateId = TemplateId,
                PlanetaryRotation = PlanetaryRotation,
                OrbitalPosition = OrbitalPosition,
                Macroeconomy = Macroeconomy,
                MeterologicalFronts = MeterologicalFronts,
                CurrentTimeOfDay = CurrentTimeOfDay,
                CelestialPositions = CelestialPositions
            };
        }
    }
}
