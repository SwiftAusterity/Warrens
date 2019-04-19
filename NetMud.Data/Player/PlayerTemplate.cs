using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Architectural.Serialization;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class PlayerTemplate : EntityTemplatePartial, IPlayerTemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Player); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.None; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                {
                    _keywords = new string[] { FullName().ToLower(), Name.ToLower(), SurName.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }


        /// <summary>
        /// Last known location Id for character in live world
        /// </summary>
        public ulong CurrentSlice { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        [StringDataIntegrity("Surname is required.")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Family Name", Description = "Last Name for you in-game.")]
        [DataType(DataType.Text)]
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        [Display(Name = "Chosen Role", Description = "The administrative role.")]
        [UIHint("EnumDropDownList")]
        public StaffRank GamePermissionsRank { get; set; }


        /// <summary>
        /// The last known location Id this character was seen in by system (for restore/backup purposes)
        /// </summary>
        [JsonConverter(typeof(ConcreteTypeConverter<GlobalPosition>))]
        [NonNullableDataIntegrity("Missing location data.")]
        public IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Account handle (user) this belongs to
        /// </summary>
        public string AccountHandle { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _account { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Missing account data.")]
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(AccountHandle))
                {
                    _account = Players.Account.GetByHandle(AccountHandle);
                }

                return _account;
            }
        }

        public ulong TotalHealth { get; set; }
        public int TotalStamina { get; set; }

        /// <summary>
        /// fArt Combos
        /// </summary>
        public HashSet<IFightingArtCombination> Combos { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public PlayerTemplate()
        {
            TotalHealth = 100;
            TotalStamina = 100;
            Qualities = new HashSet<IQuality>();
            Combos = new HashSet<IFightingArtCombination>();
        }

        [JsonConstructor]
        public PlayerTemplate(GlobalPosition currentLocation)
        {
            CurrentLocation = currentLocation;
            TotalHealth = 100;
            TotalStamina = 100;
            Qualities = new HashSet<IQuality>();
            Combos = new HashSet<IFightingArtCombination>();
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public override CacheType CachingType => CacheType.PlayerData;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                PlayerDataCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        #region data persistence
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                //reset this guy's Id to the next one in the list
                GetNextId();
                Created = DateTime.Now;
                Creator = creator;
                CreatorRank = rank;

                //No approval stuff necessary here
                ApproveMe(creator, rank);

                PersistToCache();
                accessor.WriteCharacter(this);
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
        public override IKeyedData SystemCreate()
        {
            DataAccess.FileSystem.PlayerData accessor = new DataAccess.FileSystem.PlayerData();

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
                    accessor.WriteCharacter(this);
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
        public override bool Remove(IAccount remover, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                //Remove from cache first
                PlayerDataCache.Remove(new PlayerDataCacheKey(this));

                //Remove it from the file system.
                accessor.ArchiveCharacter(this);
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
        public override bool Save(IAccount editor, StaffRank rank)
        {
            DataAccess.FileSystem.PlayerData accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                //No approval stuff necessary here
                ApproveMe(editor, rank);

                LastRevised = DateTime.Now;

                PersistToCache();
                accessor.WriteCharacter(this);
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
        public override bool SystemSave()
        {
            DataAccess.FileSystem.PlayerData accessor = new DataAccess.FileSystem.PlayerData();

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
                    accessor.WriteCharacter(this);
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
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new PlayerTemplate
            {
                Name = Name,
                AccountHandle = AccountHandle,
                GamePermissionsRank = GamePermissionsRank,
                Qualities = Qualities,
                SurName = SurName,
                StillANoob = StillANoob,
                TotalHealth = TotalHealth,
                TotalStamina = TotalStamina
            };
        }
        #endregion
    }
}
