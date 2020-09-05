using Lucene.Net.Linq.Mapping;
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
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural
{
    /// <summary>
    /// Configuration data. Only one of these spawns forever
    /// </summary>
    [Serializable]
    public abstract class LuceneData : SerializableDataPartial, ILuceneData
    {
        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public virtual string UniqueKey => Name;

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The Name of the data type.")]
        [DataType(DataType.Text)]
        [Required]
        [Field(Key = true, CaseSensitive = false)]
        public virtual string Name { get; set; }

        #region Approval System
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [IgnoreField]
        public virtual ContentApprovalType ApprovalType => ContentApprovalType.Admin; //Config data defaults to admin

        /// <summary>
        /// Is this able to be seen and used for live purposes
        /// </summary>
        [Field]
        public bool SuitableForUse => State == ApprovalState.Approved || ApprovalType == ContentApprovalType.None || ApprovalType == ContentApprovalType.ReviewOnly;

        /// <summary>
        /// Has this been approved?
        /// </summary>
        [Field]
        public ApprovalState State { get; set; }

        /// <summary>
        /// When was this approved
        /// </summary>
        [Field]
        public DateTime ApprovedOn { get; set; }

        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        [Field(CaseSensitive = false)]
        public string CreatorHandle { get; set; }

        /// <summary>
        /// The creator's account permissions level
        /// </summary>
        [Field]
        public StaffRank CreatorRank { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        [IgnoreField]
        private IAccount _creator { get; set; }

        /// <summary>
        /// Who created this thing
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [IgnoreField]
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
        [Field(CaseSensitive = false)]
        public string ApproverHandle { get; set; }

        /// <summary>
        /// The approver's account permissions level
        /// </summary>
        [Field]
        public StaffRank ApproverRank { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        [IgnoreField]
        private IAccount _approvedBy { get; set; }

        /// <summary>
        /// Who approved this thing
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [IgnoreField]
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
            return rank == StaffRank.Admin || rank >= CreatorRank || Creator.Equals(approver);
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

            DataAccess.FileSystem.LuceneData accessor = new DataAccess.FileSystem.LuceneData();
            ApproveMe(approver, rank, newState);

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
                { "Creator Rank", CreatorRank.ToString() }
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

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public virtual CacheType CachingType => CacheType.LuceneData;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                LuceneDataCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        #region Data persistence functions
        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Remove(IAccount remover, StaffRank rank)
        {
            DataAccess.FileSystem.LuceneData accessor = new DataAccess.FileSystem.LuceneData();

            try
            {
                //Not allowed to remove stuff you didn't make unless you're an admin, TODO: Make this more nuanced for guilds
                if (rank < StaffRank.Admin && !remover.Equals(Creator))
                {
                    return false;
                }

                //Remove from cache first
                LuceneDataCache.Remove(new LuceneDataCacheKey(this));

                //Remove it from the file system.
                accessor.RemoveEntity(this);
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
            DataAccess.FileSystem.LuceneData accessor = new DataAccess.FileSystem.LuceneData();

            try
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

                if (Creator == null)
                {
                    Creator = editor;
                    CreatorRank = rank;
                }

                PersistToCache();
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
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool SystemSave()
        {
            DataAccess.FileSystem.LuceneData accessor = new DataAccess.FileSystem.LuceneData();

            try
            {
                if (string.IsNullOrWhiteSpace(CreatorHandle))
                {
                    CreatorHandle = DataHelpers.SystemUserHandle;
                }

                //only able to edit its own crap
                if (CreatorHandle != DataHelpers.SystemUserHandle)
                {
                    return false;
                }

                State = ApprovalState.Approved;
                ApproverHandle = DataHelpers.SystemUserHandle;
                ApprovedOn = DateTime.Now;
                ApproverRank = StaffRank.Builder;

                PersistToCache();
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
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool SystemRemove()
        {
            DataAccess.FileSystem.LuceneData accessor = new DataAccess.FileSystem.LuceneData();

            try
            {
                //Remove from cache first
                LuceneDataCache.Remove(new LuceneDataCacheKey(this));

                //Remove it from the file system.
                accessor.RemoveEntity(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }
        #endregion

        public abstract object Clone();
    }
}
