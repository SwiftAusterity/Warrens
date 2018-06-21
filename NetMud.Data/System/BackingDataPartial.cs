using NetMud.Data.DataIntegrity;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
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
    public abstract class BackingDataPartial : SerializableDataPartial, IKeyedData
    {
        #region Data ID Parameters
        /// <summary>
        /// Numerical iterative Id in the db
        /// </summary>
        [LongDataIntegrity("Id is less than zero", -1)]
        public long Id { get; set; }

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
        #endregion

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
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public virtual IList<string> FitnessReport()
        {
            var dataProblems = new List<string>();
            var requiredProperties = GetType().GetProperties().Where(prop => prop.CustomAttributes.Any(attr => attr.AttributeType.BaseType == typeof(BaseDataIntegrity)));

            //Sift through the props decorated with DataIntegrity Attributes
            foreach (var property in requiredProperties)
            {
                foreach (var checker in property.GetCustomAttributes(typeof(BaseDataIntegrity), false))
                {
                    BaseDataIntegrity integrityCheck = (BaseDataIntegrity)checker;

                    if (!integrityCheck.Verify(property.GetValue(this)))
                        dataProblems.Add(integrityCheck.ErrorMessage);
                }
            }

            return dataProblems;
        }

        #region Approval System
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract ContentApprovalType ApprovalType { get; }

        /// <summary>
        /// Has this been approved?
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// When was this approved
        /// </summary>
        public DateTime ApprovedOn { get; set; }

        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        public string CreatorHandle { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _creator { get; set; }

        /// <summary>
        /// Who created this thing
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IAccount Creator
        {
            get
            {
                if (_creator == null && !string.IsNullOrWhiteSpace(CreatorHandle))
                    _creator = Account.GetByHandle(CreatorHandle);

                return _creator;
            }
            set
            {
                if (value != null)
                    CreatorHandle = value.GlobalIdentityHandle;
                else
                    CreatorHandle = string.Empty;

                _creator = value;
            }
        }

        /// <summary>
        /// Who approved this thing, their GlobalAccountHandle
        /// </summary>
        public string ApproverHandle { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _approvedBy { get; set; }

        /// <summary>
        /// Who approved this thing
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IAccount ApprovedBy
        {
            get
            {
                if (_approvedBy == null && !string.IsNullOrWhiteSpace(ApproverHandle))
                    _approvedBy = Account.GetByHandle(ApproverHandle);

                return _approvedBy;
            }
            set
            {
                if (value != null)
                    ApproverHandle = value.GlobalIdentityHandle;
                else
                    ApproverHandle = string.Empty;

                _approvedBy = value;
            }
        }
        #endregion  

        #region Data persistence functions
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public virtual IKeyedData Create()
        {
            var accessor = new DataAccess.FileSystem.BackingData();

            try
            {
                if (Created != DateTime.MinValue)
                    Save();
                else
                {

                    //reset this guy's Id to the next one in the list
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
                BackingDataCache.Remove(new BackingDataCacheKey(this));

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
        /// Grabs the next Id in the chain of all objects of this type.
        /// </summary>
        internal void GetNextId()
        {
            IEnumerable<IKeyedData> allOfMe = BackingDataCache.GetAll().Where(bdc => bdc.GetType() == GetType());

            //Zero ordered list
            if (allOfMe.Count() > 0)
                Id = allOfMe.Max(dp => dp.Id) + 1;
            else
                Id = 0;
        }
        #endregion

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IKeyedData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Id.Equals(Id))
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
        public bool Equals(IKeyedData other)
        {
            if (other != default(IKeyedData))
            {
                try
                {
                    return other.GetType() == GetType() && other.Id.Equals(Id);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IKeyedData x, IKeyedData y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(IKeyedData obj)
        {
            return obj.GetType().GetHashCode() + obj.Id.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Id.GetHashCode();
        }
        #endregion
    }
}
