using NetMud.Data.DataIntegrity;
using NetMud.Data.EntityBackingData;
using NetMud.Data.Lexical;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Gaia.Geographical;
using NetMud.Gaia.Meteorological;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Room : LocationEntityPartial, IRoom
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string DataTemplateName
        {
            get
            {
                return DataTemplate<IRoomData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IRoomData), DataTemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        [JsonProperty("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Rooms must have a locale affiliation.")]
        public ILocale ParentLocation
        {
            get
            {
                return LiveCache.Get<ILocale>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomData room)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();

            DataTemplateId = room.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Gets the remaining distance and next "step" to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) and the next path you'd have to use</returns>
        public Tuple<int, IPathway> GetDistanceAndNextStepDestination(ILocation destination)
        {
            var distance = -1;
            IPathway nextStep = null;

            return new Tuple<int, IPathway>(distance, nextStep);
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            var dT = DataTemplate<IRoomData>();
            var zone = AbsolutePosition().GetZone();

            var canSeeSky = GeographicalUtilities.IsOutside(GetBiome()) 
                            && dT.Coordinates.Item3 >= zone.DataTemplate<IZoneData>().BaseElevation;

            //if (!canSeeSky)
            //    return Enumerable.Empty<ICelestial>();

            //The zone knows about the celestial positioning
            return zone.GetVisibileCelestials(viewer);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            var zone = AbsolutePosition().GetZone();
            float lumins = zone.GetCurrentLuminosity();

            foreach (var dude in MobilesInside.EntitiesContained())
                lumins += dude.GetCurrentLuminosity();

            foreach (var thing in Contents.EntitiesContained())
                lumins += thing.GetCurrentLuminosity();

            return lumins;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return this;
        }

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IOccurrence RenderToLook(IEntity viewer)
        {
            return GetFullDescription(viewer, new[] { MessagingType.Visible });
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override IOccurrence GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            //Self becomes the first sense in the list
            IOccurrence me = null;
            foreach (var sense in sensoryTypes)
            {
                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var aDescs = GetAudibleDescriptives(viewer);

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSounds = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSounds = collectiveSounds.TryModify(LexicalType.Verb, GrammaticalType.Verb, "hear");
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSounds.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSounds);
                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var oDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSmells = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSmells = collectiveSmells.TryModify(LexicalType.Verb, GrammaticalType.Verb, "smell");
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSmells);
                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var pDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectivePsy = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberPsy = collectivePsy.TryModify(LexicalType.Verb, GrammaticalType.Verb, "sense");
                        uberPsy.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectivePsy);
                        break;
                    case MessagingType.Taste:
                        if (!IsTastableTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var taDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveTaste = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberTaste = collectiveTaste.TryModify(LexicalType.Verb, GrammaticalType.Verb, "taste");
                        uberTaste.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTaste.TryModify(newDesc);
                        }

                        if (uberTaste.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveTaste);
                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var tDescs = GetTouchDescriptives(viewer);

                        me.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveTouch = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberTouch = collectiveTouch.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feel");
                        uberTouch.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTouch.TryModify(newDesc);
                        }

                        if (uberTouch.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveTouch);
                        break;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSight = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSight = collectiveSight.TryModify(LexicalType.Verb, GrammaticalType.Verb, "see");
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSight);
                        break;
                }
            }

            //If we get through that and me is still null it means we can't detect anything at all
            if (me == null)
                return new Occurrence(sensoryTypes[0]);

            foreach (var celestial in GetVisibileCelestials(viewer))
                me.TryModify(celestial.RenderAsContents(viewer, sensoryTypes).Event);

            if (NaturalResources != null)
                foreach (var resource in NaturalResources)
                    me.TryModify(resource.Key.RenderResourceCollection(viewer, resource.Value).Event);

            foreach (var path in GetPathways())
                me.TryModify(path.RenderAsContents(viewer, sensoryTypes).Event);

            foreach (var obj in GetContents<IInanimate>())
                me.TryModify(obj.RenderAsContents(viewer, sensoryTypes).Event);

            foreach (var mob in GetContents<IMobile>().Where(player => !player.Equals(viewer)))
                me.TryModify(mob.RenderAsContents(viewer, sensoryTypes).Event);

            //Describe the size and population of this zone
            var roomSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

            var area = new Lexica(LexicalType.Noun, GrammaticalType.Subject, "space");
            area.TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "this");
            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, roomSize.ToString());

            //Add the temperature
            area.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Lexica[] {
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString()),
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString())
            });

            //Render people in the zone
            var populationSize = GeographicalUtilities.GetCrowdSize(GetContents<IMobile>().Count());

            var crowdSize = "lonely";
            if ((short)populationSize > (short)roomSize)
                crowdSize = "crowded";
            else if (populationSize > CrowdSizeDescription.Intimate)
                crowdSize = "sparsely populated";

            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, crowdSize);

            me.TryModify(area);

            return me;
        }

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(int radius, bool visibleOnly)
        {
            //TODO: fix visibility
            return Cartography.Rendering.RenderRadiusMap(this, 3);
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IRoom>(DataTemplateId, typeof(RoomData));

            //Isn't in the world currently
            if (me == default(IRoom))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Contents = me.Contents;
                MobilesInside = me.MobilesInside;
                Keywords = me.Keywords;
                NaturalResources = me.NaturalResources;
                ParentLocation = me.ParentLocation;
                Model = me.Model;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<IRoomData>() ?? throw new InvalidOperationException("Missing backing data store on room spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };

            if (NaturalResources == null)
                NaturalResources = new Dictionary<INaturalResource, int>();

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            if (spawnTo?.CurrentLocation == null)
                spawnTo = new GlobalPosition(this);

            CurrentLocation = spawnTo;
            ParentLocation = bS.ParentLocation.GetLiveInstance();
            Model = bS.Model;

            UpsertToLiveWorldCache(true);
        }
        #endregion
    }
}
