using NetMud.Data.DataIntegrity;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
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
        /// The system type for the entity this attaches to
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Game.Inanimate); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                    _keywords = new string[] { Name.ToLower() };

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical model is invalid.")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        public HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        public HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        [JsonProperty("InternalComposition")]
        private IDictionary<BackingDataCacheKey, short> _internalComposition { get; set; }

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

                _internalComposition = value.ToDictionary(k => new BackingDataCacheKey(k.Key), k => k.Value);
            }
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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (InternalComposition == null)
                dataProblems.Add("Internal composition cluster is null.");
            else
            {
                if (InternalComposition.Any(kvp => kvp.Key == null))
                    dataProblems.Add("Internal composition key object is null.");

                if (InternalComposition.Any(kvp => kvp.Value < 0))
                    dataProblems.Add("Internal composition value is invalid.");
            };

            return dataProblems;
        }
    }
}
