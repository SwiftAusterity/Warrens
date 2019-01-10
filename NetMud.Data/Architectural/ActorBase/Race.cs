using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.ActorBase
{
    /// <summary>
    /// Character race, determines loads of things
    /// </summary>
    [Serializable]
    public class Race : LookupDataPartial, IRace
    {
        [JsonProperty("Arms")]
        private Tuple<TemplateCacheKey, short> _arms { get; set; }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// The arm objects
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Tuple<IInanimateTemplate, short> Arms
        {
            get
            {
                if (_arms != null)
                    return new Tuple<IInanimateTemplate, short>(TemplateCache.Get<IInanimateTemplate>(_arms.Item1), _arms.Item2);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _arms = new Tuple<TemplateCacheKey, short>(new TemplateCacheKey(value.Item1), value.Item2);
            }
        }

        [JsonProperty("Legs")]
        private Tuple<TemplateCacheKey, short> _legs { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public Tuple<IInanimateTemplate, short> Legs
        {
            get
            {
                if (_legs != null)
                    return new Tuple<IInanimateTemplate, short>(TemplateCache.Get<IInanimateTemplate>(_legs.Item1), _legs.Item2);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _legs = new Tuple<TemplateCacheKey, short>(new TemplateCacheKey(value.Item1), value.Item2);
            }
        }

        [JsonProperty("Torso")]
        private TemplateCacheKey _torso { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Torso is invalid.")]
        public IInanimateTemplate Torso
        {
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_torso);
            }
            set
            {
                _torso = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Head")]
        private TemplateCacheKey _head { get; set; }

        /// <summary>
        /// The head object
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Head is invalid.")]
        public IInanimateTemplate Head
        {
            get
            {
                return TemplateCache.Get<IInanimateTemplate>(_head);
            }
            set
            {
                _head = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("BodyParts")]
        private HashSet<Tuple<TemplateCacheKey, short, string>> _bodyParts { get; set; }

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<Tuple<IInanimateTemplate, short, string>> BodyParts
        {
            get
            {
                if (_legs != null)
                    return _bodyParts.Select(bp => new Tuple<IInanimateTemplate, short, string>(TemplateCache.Get<IInanimateTemplate>(bp.Item1), bp.Item2, bp.Item3));

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _bodyParts = new HashSet<Tuple<TemplateCacheKey, short, string>>(value.Select(bp => new Tuple<TemplateCacheKey, short, string>(new TemplateCacheKey(bp.Item1), bp.Item2, bp.Item3)));
            }
        }

        /// <summary>
        /// Dietary type of this race
        /// </summary>
        public DietType DietaryNeeds { get; set; }

        [JsonProperty("SanguinaryMaterial")]
        private TemplateCacheKey _sanguinaryMaterial { get; set; }

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
                return TemplateCache.Get<IMaterial>(_sanguinaryMaterial);
            }
            set
            {
                _sanguinaryMaterial = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Low and High luminosity vision range
        /// </summary>
        public ValueRange<short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        public ValueRange<short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        public RespiratoryType Breathes { get; set; }

        /// <summary>
        /// The type of damage biting inflicts
        /// </summary>
        public DamageType TeethType { get; set; }

        /// <summary>
        /// The name used to describe a large gathering of this race
        /// </summary>
        [StringDataIntegrity("Races must have a collective noun between 2 and 50 characters long.", 2, 50)]
        public string CollectiveNoun { get; set; }

        [JsonProperty("StartingLocation")]
        private TemplateCacheKey _startingLocation { get; set; }

        /// <summary>
        /// What is the starting room of new players
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Starting Location is invalid.")]
        public IZoneTemplate StartingLocation
        {
            get
            {
                return TemplateCache.Get<IZoneTemplate>(_startingLocation);
            }
            set
            {
                _startingLocation = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("EmergencyLocation")]
        private TemplateCacheKey _emergencyLocation { get; set; }

        /// <summary>
        /// When a player loads without a location where do we send them
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Emergency Location is invalid.")]
        public IZoneTemplate EmergencyLocation
        {
            get
            {
                return TemplateCache.Get<IZoneTemplate>(_emergencyLocation);
            }
            set
            {
                _emergencyLocation = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Make a new blank race
        /// </summary>
        public Race()
        {
            BodyParts = Enumerable.Empty<Tuple<IInanimateTemplate, short, string>>();
        }

        /// <summary>
        /// Method to get the full list of anatomical features of this race
        /// </summary>
        public IEnumerable<Tuple<IInanimateTemplate, string>> FullAnatomy()
        {
            var anatomy = new List<Tuple<IInanimateTemplate, string>>();

            if (Arms.Item1 != null)
            {
                var i = 1;
                while (i < Arms.Item2)
                {
                    anatomy.Add(new Tuple<IInanimateTemplate, string>(Arms.Item1, string.Format("Arm {0}", i.ToGreek(true))));
                    i++;
                }
            }

            if (Legs.Item1 != null)
            {
                var i = 1;
                while (i < Arms.Item2)
                {
                    anatomy.Add(new Tuple<IInanimateTemplate, string>(Legs.Item1, string.Format("Leg {0}", i.ToGreek(true))));
                    i++;
                }
            }

            if (Head != null)
                anatomy.Add(new Tuple<IInanimateTemplate, string>(Head, "Head"));

            if (Torso != null)
                anatomy.Add(new Tuple<IInanimateTemplate, string>(Torso, "Torso"));

            foreach (var bit in BodyParts)
                anatomy.Add(new Tuple<IInanimateTemplate, string>(bit.Item1, bit.Item3));

            return anatomy;
        }

        /// <summary>
        /// Render this race's body as an ascii.. thing
        /// </summary>
        /// <returns>List of strings as rows for rendering</returns>
        public IEnumerable<string> RenderAnatomy(bool forWeb)
        {
            var stringList = new List<string>();

            if (Head != null)
                stringList.Add(Head.Model.ModelTemplate.ViewFlattenedModel(forWeb));

            if (Arms.Item1 != null)
            {
                var armCount = 0;
                while(armCount < Arms.Item2)
                {
                    armCount++;
                    stringList.Add(Arms.Item1.Model.ModelTemplate.ViewFlattenedModel(forWeb));
                }
            }

            if (Head != null)
                stringList.Add(Torso.Model.ModelTemplate.ViewFlattenedModel(forWeb));

            if (Legs.Item1 != null)
            {
                var legCount = 0;
                while (legCount < Legs.Item2)
                {
                    legCount++;
                    stringList.Add(Legs.Item1.Model.ModelTemplate.ViewFlattenedModel(forWeb));
                }
            }

            foreach (var bit in BodyParts)
            {
                if (bit.Item1 == null)
                    continue;

                for(var i = 0; i < bit.Item2; i++)
                    stringList.Add(bit.Item1.Model.ModelTemplate.ViewFlattenedModel(forWeb));
            }

            return stringList;
        }

        /// <summary>
        /// Gets the errors for Template fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your Template is</returns>
        public override IList<string> FitnessReport()
        {
            var TemplateProblems = base.FitnessReport();

            //Gotta keep most of these in due to the tuple thing
            if (Arms == null || Arms.Item1 == null || Arms.Item2 < 0)
                TemplateProblems.Add("Arms are invalid.");

            if (Legs == null || Legs.Item1 == null || Legs.Item2 < 0)
                TemplateProblems.Add("Legs are invalid.");

            if (BodyParts != null && BodyParts.Any(a => a.Item1 == null || a.Item2 == 0 || string.IsNullOrWhiteSpace(a.Item3)))
                TemplateProblems.Add("BodyParts are invalid.");

            if (VisionRange == null || VisionRange.Low >= VisionRange.High)
                TemplateProblems.Add("Vision range is invalid.");

            if (TemperatureTolerance == null || TemperatureTolerance.Low >= TemperatureTolerance.High)
                TemplateProblems.Add("Temperature tolerance is invalid.");

            return TemplateProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Collective", CollectiveNoun);
            returnList.Add("Starting Zone", StartingLocation.Name);
            returnList.Add("Recall Zone", EmergencyLocation.Name);

            returnList.Add("Head", Head.Name);
            returnList.Add("Torso", Torso.Name);
            returnList.Add("Legs", string.Format("{1} {0}", Legs.Item1.Name, Legs.Item2));
            returnList.Add("Arms", string.Format("{1} {0}", Arms.Item1.Name, Arms.Item2));
            returnList.Add("Blood", SanguinaryMaterial.Name);
            returnList.Add("Teeth", TeethType.ToString());
            returnList.Add("Breathes", Breathes.ToString());
            returnList.Add("Diet", DietaryNeeds.ToString());
            returnList.Add("Vision Range", string.Format("{0} - {1}", VisionRange.Low, VisionRange.High));
            returnList.Add("Temperature Tolerance", string.Format("{0} - {1}", TemperatureTolerance.Low, TemperatureTolerance.High));

            foreach (var part in BodyParts)
                returnList.Add("Body Parts", string.Format("{0} - {1} ({2})", part.Item3.ToString(), part.Item1.Name, part.Item3));

            return returnList;
        }
    }
}
