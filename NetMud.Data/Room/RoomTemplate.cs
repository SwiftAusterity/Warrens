using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    [Serializable]
    public class RoomTemplate : LocationTemplateEntityPartial, IRoomTemplate
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Room); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Leader; } }

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
                    _keywords = new string[] { Name.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        [JsonProperty("Medium")]
        private TemplateCacheKey _medium { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public RoomTemplate()
        {
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            return dataProblems;
        }

        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        public int GetDistanceDestination(ILocationData destination)
        {
            return -1;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return LiveCache.Get<IRoom>(Id);
        }

        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            //approval will be handled inside the base call
            IKeyedData obj = base.Create(creator, rank);

            return obj;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            return returnList;
        }

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
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
    }
}
