using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace NetMud.Data.System
{
    public abstract class SerializableDataPartial : IFileStored, IData
    {
        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        public virtual string Serialize()
        {
            var serializer = JsonSerializer.Create();

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, this);

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a json string into this entity
        /// </summary>
        /// <param name="jsonData">string to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored DeSerialize(string jsonData)
        {
            var serializer = JsonSerializer.Create();

            var reader = new StringReader(jsonData);

            return serializer.Deserialize(reader, this.GetType()) as IFileStored;
        }

        /// <summary>
        /// Serialize this live entity to a binary stream
        /// </summary>
        /// <returns>binary stream</returns>
        public virtual byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(Serialize());
        }

        /// <summary>
        /// Deserialize a binary stream into this entity
        /// </summary>
        /// <param name="bytes">binary to deserialize</param>
        /// <returns>the entity</returns>
        public virtual IFileStored FromBytes(byte[] bytes)
        {
            var strData = Encoding.ASCII.GetString(bytes);

            return DeSerialize(strData);
        }

        /// <summary>
        /// Numerical iterative ID in the db
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// When this was first created in the db
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// When this was last revised in the db
        /// </summary>
        public DateTime LastRevised { get; set; }

        /// <summary>
        /// The unique name for this entry (also part of the accessor keywords)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public abstract void Fill(global::System.Data.DataRow dr);

        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public virtual IData Create()
        {
            var accessor = new NetMud.DataAccess.FileSystem.BackingData();

            try
            {
                BackingDataCache.Add(this);
                accessor.WriteEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return null;
            }

            return this;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Remove()
        {
            var accessor = new NetMud.DataAccess.FileSystem.BackingData();

            try
            {
                //Remove from cache first
                BackingDataCache.Remove(new BackingDataCacheKey(this.GetType(), this.ID));

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
            var accessor = new NetMud.DataAccess.FileSystem.BackingData();

            try
            {
                accessor.WriteEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.ID.Equals(this.ID))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IData other)
        {
            if (other != default(IData))
            {
                try
                {
                    return other.GetType() == this.GetType() && other.ID.Equals(this.ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

    }
}
