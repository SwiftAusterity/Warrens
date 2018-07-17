using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
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
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override float GetVisionModifier()
        {
            //Base case doesn't count "lumin vision range" mobiles/players have, inanimate entities are assumed to have unlimited light and dark vision

            //TODO: Check for blindess/magical type affects

            return DataTemplate<IInanimateData>().VisualAcuity;
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
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var backingStore = DataTemplate<IInanimateData>();

            var sb = new List<string>
            {
                string.Format("There is a {0} here", backingStore.Name)
            };

            return sb;
        }

        public IEnumerable<string> RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var sb = new List<string>
            {
                string.Format("{0} is wearing {1}", wearer.DataTemplateName, GetDescribedName(viewer))
            };

            return sb;
        }

        public IEnumerable<string> RenderAsHeld(IEntity viewer, IEntity holder)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var sb = new List<string>
            {
                string.Format("{0} is holding {1}", holder.DataTemplateName, GetDescribedName(viewer))
            };

            return sb;
        }
        #endregion
    }
}
