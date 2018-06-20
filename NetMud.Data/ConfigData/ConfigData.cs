using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using System;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// Configuration data. Only one of these spawns forever
    /// </summary>
    [Serializable]
    public abstract class ConfigData : SerializableDataPartial, IConfigData
    {
        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        public abstract ConfigDataType Type { get; }

        #region Data persistence functions
        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Remove()
        {
            var accessor = new DataAccess.FileSystem.ConfigData();

            try
            {
                //Remove from cache first
                ConfigDataCache.Remove(new ConfigDataCacheKey(this));

                //Remove it from the file system.
                accessor.ArchiveEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Save()
        {
            var accessor = new DataAccess.FileSystem.ConfigData();

            try
            {
                ConfigDataCache.Add(this);
                accessor.WriteEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }
        #endregion
    }
}
