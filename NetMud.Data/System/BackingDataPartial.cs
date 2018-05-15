using NetMud.Data.DataIntegrity;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    /// <summary>
    /// Partial for all backing data
    /// </summary>
    public abstract class BackingDataPartial : SerializableDataPartial, IData
    {
        /// <summary>
        /// Numerical iterative ID in the db
        /// </summary>
        [LongDataIntegrity("ID is less than zero", -1)]
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
        [StringDataIntegrity("Name is blank")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public virtual IList<string> FitnessReport()
        {
            var dataProblems = new List<string>();
            var requiredProperties = GetType().GetProperties().Where(prop => prop.CustomAttributes.Any(attr => attr.AttributeType.BaseType == typeof(BaseDataIntegrity)));

            foreach (var property in requiredProperties)
            {
                foreach (var checker in property.GetCustomAttributes(typeof(BaseDataIntegrity), false))
                {
                    BaseDataIntegrity integrityCheck = (BaseDataIntegrity)checker;

                    if (!integrityCheck.Verify(property.GetValue(this)))
                        dataProblems.Add(integrityCheck.ErrorMessage);
                }
            }

            //if (String.IsNullOrWhiteSpace(Name))
            //    dataProblems.Add("Name is blank.");

            //if (ID < 0)
            //    dataProblems.Add("ID is less than zero.");
    
            return dataProblems;
        }

        /// <summary>
        /// Does this have data problems?
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public bool FitnessProblems 
        { 
            get
            {
                return FitnessReport().Any();
            }
        }

        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public virtual IData Create()
        {
            var accessor = new DataAccess.FileSystem.BackingData();

            try
            {
                if (Created != DateTime.MinValue)
                    Save();
                else
                {

                    //reset this guy's ID to the next one in the list
                    GetNextId();
                    Created = DateTime.Now;

                    BackingDataCache.Add(this);
                    accessor.WriteEntity(this);
                }
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
            var accessor = new DataAccess.FileSystem.BackingData();

            try
            {
                //Remove from cache first
                BackingDataCache.Remove(new BackingDataCacheKey(GetType(), ID));

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
            var accessor = new DataAccess.FileSystem.BackingData();

            try
            {
                if (Created == DateTime.MinValue)
                    Create();
                else
                {
                    LastRevised = DateTime.Now;

                    BackingDataCache.Add(this);
                    accessor.WriteEntity(this);
                }
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
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.ID.Equals(ID))
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
                    return other.GetType() == GetType() && other.ID.Equals(ID);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Grabs the next ID in the chain of all objects of this type.
        /// </summary>
        internal void GetNextId()
        {
            IEnumerable<IData> allOfMe = BackingDataCache.GetAll().Where(bdc => bdc.GetType() == GetType());

            //Zero ordered list
            if (allOfMe.Count() > 0)
                ID = allOfMe.Max(dp => dp.ID) + 1;
            else
                ID = 0;
        }
    }
}
