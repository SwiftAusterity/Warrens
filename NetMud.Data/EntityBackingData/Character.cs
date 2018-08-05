using NetMud.Data.ConfigData;
using NetMud.Data.DataIntegrity;
using NetMud.Data.Lexical;
using NetMud.Data.Serialization;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class Character : EntityBackingDataPartial, ICharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Game.Player); }
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
                    _keywords = new string[] { FullName().ToLower(), Name.ToLower(), SurName.ToLower() };

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Gender data string for player characters
        /// </summary>
        [StringDataIntegrity("Gender is required.")]
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        [StringDataIntegrity("Surname is required.")]
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        public IDictionary<MessagingType, bool> SuperSenses { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        [JsonProperty("RaceData")]
        private BackingDataCacheKey _raceData { get; set; }

        /// <summary>
        /// The race data for the character
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Missing racial data.")]
        public IRace RaceData 
        { 
            get
            {
                return BackingDataCache.Get<IRace>(_raceData);
            }
            set
            {
                _raceData = new BackingDataCacheKey(value);
            }
        }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
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
                    _account = System.Account.GetByHandle(AccountHandle);

                return _account;
            }
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public Character()
        {
            SuperSenses = new Dictionary<MessagingType, bool>();
        }

        [JsonConstructor]
        public Character(GlobalPosition currentLocation)
        {
            SuperSenses = new Dictionary<MessagingType, bool>();
            CurrentLocation = currentLocation;
        }

        /// <summary>
        /// Full name to refer to this NPC with
        /// </summary>
        /// <returns>the full name string</returns>
        public string FullName()
        {
            return string.Format("{0} {1}", Name, SurName);
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            try
            {
                if (RaceData == null)
                    return new Tuple<int, int, int>(0, 0, 0);

                var height = RaceData.Head.Model.Height + RaceData.Torso.Model.Height + RaceData.Legs.Item1.Model.Height;
                var length = RaceData.Torso.Model.Length;
                var width = RaceData.Torso.Model.Width;

                return new Tuple<int, int, int>(height, length, width);
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return new Tuple<int, int, int>(0, 0, 0);
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
                var dictatas = new List<IDictata>
                {
                    new Dictata(new Lexica(LexicalType.ProperNoun, GrammaticalType.Subject, SurName)),
                    new Dictata(new Lexica(LexicalType.ProperNoun, GrammaticalType.Subject, FullName())),
                    new Dictata(new Lexica(LexicalType.ProperNoun, GrammaticalType.Subject, Name)),
                };
                dictatas.AddRange(Descriptives.Select(desc => desc.Event.GetDictata()));

                foreach (var dictata in dictatas)
                    LexicalProcessor.VerifyDictata(dictata);

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
            var accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                //reset this guy's Id to the next one in the list
                GetNextId();
                Created = DateTime.Now;
                Creator = creator;
                CreatorRank = rank;

                //Default godsight to all false on creation unless you're making a new administrator
                SuperSenses = new Dictionary<MessagingType, bool>
                {
                    { MessagingType.Audible, rank == StaffRank.Admin },
                    { MessagingType.Olefactory, rank == StaffRank.Admin },
                    { MessagingType.Psychic, rank == StaffRank.Admin },
                    { MessagingType.Tactile, rank == StaffRank.Admin },
                    { MessagingType.Taste, rank == StaffRank.Admin },
                    { MessagingType.Visible, rank == StaffRank.Admin }
                };

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
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove(IAccount remover, StaffRank rank)
        {
            var accessor = new DataAccess.FileSystem.PlayerData();

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
            var accessor = new DataAccess.FileSystem.PlayerData();

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
        /// Grabs the next Id in the chain of all objects of this type.
        /// </summary>
        internal override void GetNextId()
        {
            var allOfMe = Account.Characters;

            //Zero ordered list
            if (allOfMe.Count() > 0)
                Id = allOfMe.Max(dp => dp.Id) + 1;
            else
                Id = 0;
        }
        #endregion
    }
}
