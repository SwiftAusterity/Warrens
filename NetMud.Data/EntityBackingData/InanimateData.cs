using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for Inanimate objects
    /// </summary>
    [Serializable]
    public class InanimateData : EntityBackingDataPartial, IInanimateData
    {
        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Game.Inanimate); }
        }

        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        public HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        public HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }

        [JsonProperty("InternalComposition")]
        private IDictionary<long, short> _internalComposition { get; set; }

        /// <summary>
        /// The list of internal compositions for separate/explosion/sharding
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDictionary<IInanimateData, short> InternalComposition
        {
            get
            {
                if (_internalComposition != null)
                    return _internalComposition.ToDictionary(k => BackingDataCache.Get<IInanimateData>(k.Key), k => k.Value);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _internalComposition = value.ToDictionary(k => k.Key.ID, k => k.Value);
            }
        }

        /// <summary>
        /// Spawn new inanimate with its model
        /// </summary>
        [JsonConstructor]
        public InanimateData(DimensionalModel model, HashSet<EntityContainerData<IMobile>> mobileContainers, HashSet<EntityContainerData<IInanimate>> inanimateContainers)
        {
            Model = model;

            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();
            InternalComposition = new Dictionary<IInanimateData, short>();
        }

        /// <summary>
        /// Spawns a new empty inanimate object
        /// </summary>
        public InanimateData()
        {
            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();
            InternalComposition = new Dictionary<IInanimateData, short>();
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }   
    }
}
