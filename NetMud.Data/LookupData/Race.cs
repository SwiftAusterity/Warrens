using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Character race, determines loads of things
    /// </summary>
    [Serializable]
    public class Race : LookupDataPartial, IRace
    {
        [JsonProperty("Arms")]
        private Tuple<long, short> _arms { get; set; }

        /// <summary>
        /// The arm objects
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Tuple<IInanimateData, short> Arms
        {
            get
            {
                if (_arms != null)
                    return new Tuple<IInanimateData, short>(BackingDataCache.Get<IInanimateData>(_arms.Item1), _arms.Item2);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _arms = new Tuple<long, short>(value.Item1.ID, value.Item2);
            }
        }

        [JsonProperty("Legs")]
        private Tuple<long, short> _legs { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Tuple<IInanimateData, short> Legs
        {
            get
            {
                if (_legs != null)
                    return new Tuple<IInanimateData, short>(BackingDataCache.Get<IInanimateData>(_legs.Item1), _legs.Item2);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _legs = new Tuple<long, short>(value.Item1.ID, value.Item2);
            }
        }

        [JsonProperty("Torso")]
        private long _torso { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Torso is invalid.")]
        public IInanimateData Torso
        {
            get
            {
                return BackingDataCache.Get<IInanimateData>(_torso);
            }
            set
            {
                _torso = value.ID;
            }
        }

        [JsonProperty("Head")]
        private long _head { get; set; }

        /// <summary>
        /// The head object
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Head is invalid.")]
        public IInanimateData Head
        {
            get
            {
                return BackingDataCache.Get<IInanimateData>(_head);
            }
            set
            {
                _head = value.ID;
            }
        }

        [JsonProperty("BodyParts")]
        private HashSet<Tuple<long, short, string>> _bodyParts { get; set; }

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<Tuple<IInanimateData, short, string>> BodyParts
        {
            get
            {
                if (_legs != null)
                    return _bodyParts.Select(bp => new Tuple<IInanimateData, short, string>(BackingDataCache.Get<IInanimateData>(bp.Item1), bp.Item2, bp.Item3));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _bodyParts = new HashSet<Tuple<long, short, string>>(value.Select(bp => new Tuple<long, short, string>(bp.Item1.ID, bp.Item2, bp.Item3)));
            }
        }

        /// <summary>
        /// Dietary type of this race
        /// </summary>
        public DietType DietaryNeeds { get; set; }

        [JsonProperty("SanguinaryMaterial")]
        private long _sanguinaryMaterial { get; set; }

        /// <summary>
        /// Material that is the blood
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Blood material is invalid.")]
        public IMaterial SanguinaryMaterial
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_sanguinaryMaterial);
            }
            set
            {
                _sanguinaryMaterial = value.ID;
            }
        }

        /// <summary>
        /// Low and High luminosity vision range
        /// </summary>
        public Tuple<short, short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        public Tuple<short, short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        public RespiratoryType Breathes { get; set; }

        /// <summary>
        /// The type of damage biting inflicts
        /// </summary>
        public DamageType TeethType { get; set; }

        [JsonProperty("StartingLocation")]
        private long _startingLocation { get; set; }

        /// <summary>
        /// What is the starting room of new players
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Starting Location is invalid.")]
        public IZoneData StartingLocation
        {
            get
            {
                return BackingDataCache.Get<IZoneData>(_startingLocation);
            }
            set
            {
                _startingLocation = value.ID;
            }
        }

        [JsonProperty("EmergencyLocation")]
        private long _emergencyLocation { get; set; }

        /// <summary>
        /// When a player loads without a location where do we send them
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Emergency Location is invalid.")]
        public IZoneData EmergencyLocation
        {
            get
            {
                return BackingDataCache.Get<IZoneData>(_emergencyLocation);
            }
            set
            {
                _emergencyLocation = value.ID;
            }
        }

        /// <summary>
        /// Make a new blank race
        /// </summary>
        public Race()
        {
            BodyParts = Enumerable.Empty<Tuple<IInanimateData, short, string>>();
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            //Gotta keep most of these in due to the tuple thing
            if (Arms == null || Arms.Item1 == null || Arms.Item2 < 0)
                dataProblems.Add("Arms are invalid.");

            if (Legs == null || Legs.Item1 == null || Legs.Item2 < 0)
                dataProblems.Add("Legs are invalid.");

            if (BodyParts != null && BodyParts.Any(a => a.Item1 == null || a.Item2 == 0 || String.IsNullOrWhiteSpace(a.Item3)))
                dataProblems.Add("BodyParts are invalid.");

            if (VisionRange == null || VisionRange.Item1 >= VisionRange.Item2)
                dataProblems.Add("Vision range is invalid.");

            if (TemperatureTolerance == null || TemperatureTolerance.Item1 >= TemperatureTolerance.Item2)
                dataProblems.Add("Temperature tolerance is invalid.");

            return dataProblems;
        }
    }
}
