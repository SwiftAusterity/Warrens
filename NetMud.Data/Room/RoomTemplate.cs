using NetMud.Communication.Lexical;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
                    _keywords = new string[] { Name.ToLower() };

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical Model is invalid.")]
        public IDimensionalModel Model { get; set; }

        [JsonProperty("Medium")]
        private TemplateCacheKey _medium { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Medium material is invalid.")]
        [Display(Name = "Medium", Description = "What the 'empty' space of the room is made of. (likely AIR, sometimes stone or dirt)")]
        [DataType(DataType.Text)]
        public IMaterial Medium
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_medium);
            }
            set
            {
                if (value != null)
                    _medium = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("ParentLocation")]
        private TemplateCacheKey _parentLocation { get; set; }

        /// <summary>
        /// What zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Parent Location is invalid.")]
        public ILocaleTemplate ParentLocation
        {
            get
            {
                return TemplateCache.Get<ILocaleTemplate>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Coordinate Coordinates { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public RoomTemplate()
        {
        }

        /// <summary>
        /// Spawn new room with its model
        /// </summary>
        [JsonConstructor]
        public RoomTemplate(DimensionalModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (Coordinates?.X < 0 || Coordinates?.Y < 0 || Coordinates?.Z < 0)
                dataProblems.Add("Coordinates are invalid.");

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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public override ILocation GetLiveInstance()
        {
            return LiveCache.Get<IRoom>(Id);
        }

        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            //approval will be handled inside the base call
            var obj = base.Create(creator, rank);

            ParentLocation.RemapInterior();

            return obj;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Medium", Medium.Name);

            foreach (var desc in Descriptives)
                returnList.Add("Descriptives", string.Format("{0} ({1}): {2}", desc.SensoryType, desc.Strength, desc.Event.ToString()));

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
                var dictatas = new List<IDictata>
                {
                    new Dictata(new Lexica(LexicalType.Noun, GrammaticalType.Subject, Name))
                };
                dictatas.AddRange(Descriptives.Select(desc => desc.Event.GetDictata()));

                foreach (var dictata in dictatas)
                    LexicalProcessor.VerifyDictata(dictata);

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
