using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

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
        public abstract Type EntityClass { get; }

        /// <summary>
        /// Affects to add to a live entity when it is spawned
        /// </summary>
        public HashSet<IAffect> AffectsOnSpawn { get; set; }

        public EntityBackingDataPartial()
        {
            //empty instance for getting the dataTableName
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
