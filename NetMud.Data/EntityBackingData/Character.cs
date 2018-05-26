using NetMud.Data.DataIntegrity;
using NetMud.Data.Serialization;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
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
                    _account = Authentication.Account.GetByHandle(AccountHandle);

                return _account;
            }
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public Character()
        {

        }

        [JsonConstructor]
        public Character(GlobalPosition currentLocation)
        {
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

        #region data persistence
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IData Create()
        {
            var accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                PlayerDataCache.Add(this);
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
        public override bool Remove()
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
        public override bool Save()
        {
            var accessor = new DataAccess.FileSystem.PlayerData();

            try
            {
                accessor.WriteCharacter(this);
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
