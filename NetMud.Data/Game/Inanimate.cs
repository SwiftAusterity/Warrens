using NetMud.Data.Lexical;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Gaia.Geographical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// "Object" class
    /// </summary>
    [Serializable]
    public class Inanimate : LocationEntityPartial, IInanimate
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
                return DataTemplate<IInanimateData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IInanimateData), DataTemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        [JsonConstructor]
        public Inanimate(DimensionalModel model)
        {
            Model = model;

            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateData backingStore)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplateId = backingStore.Id;
            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Inanimate(IInanimateData backingStore, IGlobalPosition spawnTo)
        {
            Contents = new EntityContainer<IInanimate>(backingStore.InanimateContainers);
            MobilesInside = new EntityContainer<IMobile>(backingStore.MobileContainers);

            DataTemplateId = backingStore.Id;
            SpawnNewInWorld(spawnTo);
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
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;
            foreach (var dude in MobilesInside.EntitiesContained())
                lumins += dude.GetCurrentLuminosity();

            foreach (var thing in Contents.EntitiesContained())
                lumins += thing.GetCurrentLuminosity();

            return lumins;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            //No celestials inside an object
            return Enumerable.Empty<ICelestial>();
        }

        #region spawning
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            if(CurrentLocation == null)
                throw new NotImplementedException("Objects can't spawn to nothing");

            SpawnNewInWorld(CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<IInanimateData>() ?? throw new InvalidOperationException("Missing backing data store on object spawn event.");

            CurrentLocation = spawnTo ?? throw new NotImplementedException("Objects can't spawn to nothing");

            Keywords = new string[] { bS.Name.ToLower() };

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            spawnTo.CurrentLocation.MoveInto<IInanimate>(this);

            UpsertToLiveWorldCache(true);
        }
        #endregion

        #region rendering
        public override IOccurrence RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            //TODO: Worn position
            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        public override IOccurrence RenderAsHeld(IEntity viewer, IEntity holder)
        {
            //TODO: Worn position
            return GetImmediateDescription(viewer, MessagingType.Visible);
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

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "emanating")
                                            .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "from");

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

                        var uberSmells = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell");
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "from");

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(uberSmells);
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

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "from");

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

                        var taDescs = GetTasteDescriptives(viewer);

                        me.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var uberTaste = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "taste");
                        uberTaste.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it");

                            uberTaste.TryModify(newDesc);
                        }

                        if (uberTaste.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(uberTaste);
                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var tDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var uberTouch = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "feel");
                        uberTouch.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it");

                            uberTouch.TryModify(newDesc);
                        }

                        if (uberTouch.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(uberTouch);
                        break;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var uberSight = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "appears");
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Pronoun, GrammaticalType.IndirectObject, "it");

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(uberSight);
                        break;
                }
            }

            //If we get through that and me is still null it means we can't detect anything at all
            if (me == null)
                return new Occurrence(sensoryTypes[0]);

            //Describe the size and population of this zone
            var objectSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

            me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, objectSize.ToString());

            //Render people in the zone
            var bulgeSizeAdjective = GeographicalUtilities.GetObjectContainmentSize(GetContents<IInanimate>().Sum(obj => obj.GetModelVolume()), GetModelVolume());

            me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, bulgeSizeAdjective.ToString());

            return me;
        }
        #endregion
    }
}
