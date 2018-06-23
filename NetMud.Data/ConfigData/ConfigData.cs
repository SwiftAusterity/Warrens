using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// Configuration data. Only one of these spawns forever
    /// </summary>
    [Serializable]
    public abstract class ConfigData : SerializableDataPartial, IConfigData
    {
        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract ConfigDataType Type { get; }

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        public string Name { get; set; }

        #region Approval System
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public virtual ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } } //Config data defaults to admin

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

        private void ApproveMe(IAccount approver)
        {
            Approved = true;
            ApprovedBy = approver;
            ApprovedOn = DateTime.Now;
        }
        #endregion  

        #region Data persistence functions
        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool Remove(IAccount remover, StaffRank rank)
        {
            var accessor = new DataAccess.FileSystem.ConfigData();

            try
            {                
                //Not allowed to remove stuff you didn't make unless you're an admin, TODO: Make this more nuanced for guilds
                if (rank < StaffRank.Admin && !remover.Equals(Creator))
                {
                    return false;
                }

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
        public virtual bool Save(IAccount editor, StaffRank rank)
        {
            var accessor = new DataAccess.FileSystem.ConfigData();

            try
            {
                //Not allowed to edit stuff you didn't make unless you're an admin, TODO: Make this more nuanced for guilds
                if (ApprovalType != ContentApprovalType.None && rank < StaffRank.Admin && !editor.Equals(Creator))
                {
                    return false;
                }

                //Disapprove of things first
                Approved = false;
                ApprovedBy = null;
                ApprovedOn = DateTime.MinValue;

                //Figure out automated approvals, always throw reviewonly in there
                if (rank < StaffRank.Admin && ApprovalType != ContentApprovalType.ReviewOnly)
                {
                    switch (ApprovalType)
                    {
                        case ContentApprovalType.None:
                            ApproveMe(editor);
                            break;
                        case ContentApprovalType.Leader:
                            if (rank == StaffRank.Builder)
                                ApproveMe(editor);
                            break;
                    }
                }
                else
                {
                    //Staff Admin always get approved
                    ApproveMe(editor);
                }

                if (Creator == null)
                {
                    Creator = editor;
                }

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
