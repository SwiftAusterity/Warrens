using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.Serialization;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural
{
    /// <summary>
    /// Partial for all backing data
    /// </summary>
    public abstract class TemplatePartial : SerializableDataPartial, IKeyedData
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
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name", Description = "First Name.")]
        [DataType(DataType.Text)]
        [Required]
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
            List<string> dataProblems = new List<string>();
            IEnumerable<global::System.Reflection.PropertyInfo> requiredProperties = GetType().GetProperties().Where(prop => prop.CustomAttributes.Any(attr => attr.AttributeType.BaseType == typeof(BaseDataIntegrity)));

            //Sift through the props decorated with DataIntegrity Attributes
            foreach (global::System.Reflection.PropertyInfo property in requiredProperties)
            {
                foreach (object checker in property.GetCustomAttributes(typeof(BaseDataIntegrity), false))
                {
                    BaseDataIntegrity integrityCheck = (BaseDataIntegrity)checker;

                    if (!integrityCheck.Verify(property.GetValue(this)))
                    {
                        dataProblems.Add(integrityCheck.ErrorMessage);
                    }
                }
            }

            return dataProblems;
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public virtual CacheType CachingType => CacheType.Template;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        #region Approval System
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract ContentApprovalType ApprovalType { get; }

        /// <summary>
        /// Is this able to be seen and used for live purposes
        /// </summary>
        public bool SuitableForUse
        {
            get
            {
                return State == ApprovalState.Approved || ApprovalType == ContentApprovalType.None || ApprovalType == ContentApprovalType.ReviewOnly;
            }
        }

        /// <summary>
        /// Has this been approved?
        /// </summary>
        public ApprovalState State { get; set; }

        /// <summary>
        /// When was this approved
        /// </summary>
        public DateTime ApprovedOn { get; set; }

        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        public string CreatorHandle { get; set; }

        /// <summary>
        /// The creator's account permissions level
        /// </summary>
        public StaffRank CreatorRank { get; set; }

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
                {
                    _creator = Account.GetByHandle(CreatorHandle);
                }

                return _creator;
            }
            set
            {
                if (value != null)
                {
                    CreatorHandle = value.GlobalIdentityHandle;
                }
                else
                {
                    CreatorHandle = string.Empty;
                }

                _creator = value;
            }
        }

        /// <summary>
        /// Who approved this thing, their GlobalAccountHandle
        /// </summary>
        public string ApproverHandle { get; set; }

        /// <summary>
        /// The approver's account permissions level
        /// </summary>
        public StaffRank ApproverRank { get; set; }

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
                {
                    _approvedBy = Account.GetByHandle(ApproverHandle);
                }

                return _approvedBy;
            }
            set
            {
                if (value != null)
                {
                    ApproverHandle = value.GlobalIdentityHandle;
                }
                else
                {
                    ApproverHandle = string.Empty;
                }

                _approvedBy = value;
            }
        }

        /// <summary>
        /// Can the given rank approve this or not
        /// </summary>
        /// <param name="rank">Approver's rank</param>
        /// <returns>If it can</returns>
        public bool CanIBeApprovedBy(StaffRank rank, IAccount approver)
        {
            return rank >= CreatorRank || Creator.Equals(approver);
        }

        /// <summary>
        /// Change the approval status of this thing
        /// </summary>
        /// <returns>success</returns>
        public bool ChangeApprovalStatus(IAccount approver, StaffRank rank, ApprovalState newState)
        {
            //Can't approve/deny your own stuff
            if (rank < StaffRank.Admin && Creator.Equals(approver))
            {
                return false;
            }

            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();
            ApproveMe(approver, rank, newState);

            LastRevised = DateTime.Now;

            PersistToCache();
            accessor.WriteEntity(this);

            return true;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public virtual IDictionary<string, string> SignificantDetails()
        {
            Dictionary<string, string> returnList = new Dictionary<string, string>
            {
                { "Name", Name },
                { "Creator", CreatorHandle },
                { "Creator Rank", CreatorRank.ToString() },
                { "Valid", FitnessProblems.ToString() }
            };

            return returnList;
        }

        internal void ApproveMe(IAccount approver, StaffRank rank, ApprovalState state = ApprovalState.Approved)
        {
            State = state;
            ApprovedBy = approver;
            ApprovedOn = DateTime.Now;
            ApproverRank = rank;
        }
        #endregion  

        #region Data persistence functions
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public virtual IKeyedData Create(IAccount creator, StaffRank rank)
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                if (Created != DateTime.MinValue)
                {
                    Save(creator, rank);
                }
                else
                {

                    //reset this guy's Id to the next one in the list
                    GetNextId();
                    Created = DateTime.Now;
                    Creator = creator;
                    CreatorRank = rank;

                    //Figure out automated approvals, always throw reviewonly in there
                    if (rank < StaffRank.Admin && ApprovalType != ContentApprovalType.ReviewOnly)
                    {
                        switch (ApprovalType)
                        {
                            case ContentApprovalType.None:
                                ApproveMe(creator, rank);
                                break;
                            case ContentApprovalType.Leader:
                                if (rank == StaffRank.Builder)
                                {
                                    ApproveMe(creator, rank);
                                }

                                break;
                        }
                    }

                    PersistToCache();
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
        /// Add it to the cache and save it to the file system made by SYSTEM
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public virtual IKeyedData SystemCreate()
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                if (Created != DateTime.MinValue)
                {
                    SystemSave();
                }
                else
                {

                    //reset this guy's Id to the next one in the list
                    GetNextId();
                    Created = DateTime.Now;
                    CreatorHandle = DataHelpers.SystemUserHandle;
                    CreatorRank = StaffRank.Builder;

                    PersistToCache();
                    accessor.WriteEntity(this);
                }


                State = ApprovalState.Approved;
                ApproverHandle = DataHelpers.SystemUserHandle;
                ApprovedOn = DateTime.Now;
                ApproverRank = StaffRank.Builder;
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
        public virtual bool Remove(IAccount remover, StaffRank rank)
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                //Not allowed to remove stuff you didn't make unless you're an admin, TODO: Make this more nuanced for guilds
                if (rank <= CreatorRank && !remover.Equals(Creator))
                {
                    return false;
                }

                //Remove from cache first
                TemplateCache.Remove(new TemplateCacheKey(this));

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
        public virtual bool Save(IAccount editor, StaffRank rank)
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                if (Created == DateTime.MinValue)
                {
                    Create(editor, rank);
                }
                else
                {
                    //Not allowed to edit stuff you didn't make unless you're an admin, TODO: Make this more nuanced for guilds
                    if (ApprovalType != ContentApprovalType.None && rank < StaffRank.Admin && !editor.Equals(Creator))
                    {
                        return false;
                    }

                    //Disapprove of things first
                    State = ApprovalState.Pending;
                    ApprovedBy = null;
                    ApprovedOn = DateTime.MinValue;

                    //Figure out automated approvals, always throw reviewonly in there
                    if (rank < StaffRank.Admin && ApprovalType != ContentApprovalType.ReviewOnly)
                    {
                        switch (ApprovalType)
                        {
                            case ContentApprovalType.None:
                                ApproveMe(editor, rank);
                                break;
                            case ContentApprovalType.Leader:
                                if (rank == StaffRank.Builder)
                                {
                                    ApproveMe(editor, rank);
                                }

                                break;
                        }
                    }
                    else
                    {
                        //Staff Admin always get approved
                        ApproveMe(editor, rank);
                    }

                    LastRevised = DateTime.Now;

                    PersistToCache();
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
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool SystemSave()
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                if (Created == DateTime.MinValue)
                {
                    SystemCreate();
                }
                else
                {
                    //only able to edit its own crap
                    if (CreatorHandle != DataHelpers.SystemUserHandle)
                    {
                        return false;
                    }

                    State = ApprovalState.Approved;
                    ApproverHandle = DataHelpers.SystemUserHandle;
                    ApprovedOn = DateTime.Now;
                    ApproverRank = StaffRank.Builder;
                    LastRevised = DateTime.Now;

                    PersistToCache();
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
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool SystemRemove()
        {
            DataAccess.FileSystem.TemplateData accessor = new DataAccess.FileSystem.TemplateData();

            try
            {
                //Remove from cache first
                TemplateCache.Remove(new TemplateCacheKey(this));

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
        /// Grabs the next Id in the chain of all objects of this type.
        /// </summary>
        internal virtual void GetNextId()
        {
            IEnumerable<IKeyedData> allOfMe = TemplateCache.GetAll().Where(bdc => bdc.GetType() == GetType());

            //Zero ordered list
            if (allOfMe.Count() > 0)
            {
                Id = allOfMe.Max(dp => dp.Id) + 1;
            }
            else
            {
                Id = 0;
            }
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
            if (other != null && other.GetType() == GetType())
            {
                try
                {
                    if (other.Id.Equals(Id))
                    {
                        return 0;
                    }

                    return (int)(other.Id - Id);
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

        public abstract object Clone();
    }
}
