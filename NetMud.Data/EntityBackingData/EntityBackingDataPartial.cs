using NetMud.Data.System;
using NetMud.DataAccess; using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using System;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Base class for backing data
    /// </summary>
    [Serializable]
    public abstract class EntityBackingDataPartial : BackingDataPartial, IEntityBackingData
    {
        public EntityBackingDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public abstract Type EntityClass { get; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public abstract Tuple<int, int, int> GetModelDimensions();
    }
}
