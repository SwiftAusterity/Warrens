using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    [Serializable]
    public class Character : EntityBackingDataPartial, ICharacter
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        public override Type EntityClass
        {
            get { return typeof(Game.Player); }
        }

        /// <summary>
        /// Gender data string for player characters
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public Boolean StillANoob { get; set; }

        [JsonProperty("RaceData")]
        private long _raceData { get; set; }

        /// <summary>
        /// The race data for the character
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IRace RaceData 
        { 
            get
            {
                return BackingDataCache.Get<IRace>(_raceData);
            }
            set
            {
                _raceData = value.ID;
            }
        }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// The last known location ID this character was seen in by system (for restore/backup purposes)
        /// </summary>
        public string LastKnownLocation { get; set; }
        /// <summary>
        /// The system type of the ast known location this character was seen in by system (for restore/backup purposes)
        /// </summary>
        public string LastKnownLocationType { get; set; }

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
            var height = RaceData.Head.Model.Height + RaceData.Torso.Model.Height + RaceData.Legs.Item1.Model.Height;
            var length = RaceData.Torso.Model.Length;
            var width = RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
        }
    }
}
