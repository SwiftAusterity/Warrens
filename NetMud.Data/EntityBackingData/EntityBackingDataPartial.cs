using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Base class for backing data
    /// </summary>
    [Serializable]
    public abstract class EntityBackingDataPartial : BackingDataPartial, IEntityBackingData
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract Type EntityClass { get; }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        internal string[] _keywords;

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public abstract string[] Keywords { get; set; }

        /// <summary>
        /// Affects to add to a live entity when it is spawned
        /// </summary>
        public HashSet<IAffect> Affects { get; set; }

        /// <summary>
        /// The perceptive vision strength modifier something has
        /// </summary>
        public virtual float VisualAcuity => 100;

        public EntityBackingDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        /// <summary>
        /// Does this data have this affect
        /// </summary>
        /// <param name="affectTarget">the target of the affect</param>
        /// <returns>the affect</returns>
        public bool HasAffect(string affectTarget)
        {
            return Affects.Any(aff => aff.Target.Equals(affectTarget, StringComparison.InvariantCultureIgnoreCase)
                                        && (aff.Duration > 0 || aff.Duration == -1));
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (EntityClass == null || EntityClass.GetInterface("IEntity", true) == null)
                dataProblems.Add("Entity Class type reference is broken.");

            var dims = GetModelDimensions();
            if(dims.Item1 < 0 || dims.Item2 < 0 || dims.Item3 < 0)
                dataProblems.Add("Physical dimensions of model are invalid.");

            return dataProblems;
        }


        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Tuple<int, int, int> GetModelDimensions();
    }
}
