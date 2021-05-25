using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Inanimate;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Architectural.ActorBase
{
    /// <summary>
    /// Character race, determines loads of things
    /// </summary>
    [Serializable]
    public class Race : LookupDataPartial, IRace
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// The arm objects
        /// </summary>
        [Display(Name = "Arm Object", Description = "The # of and object that this thing's arms are made of.")]
        [UIHint("IndividualInanimateComponent")]
        public IInanimateComponent Arms { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        [Display(Name = "Leg Object", Description = "The # of and object that this thing's legs are made of.")]
        [UIHint("IndividualInanimateComponent")]
        public IInanimateComponent Legs { get; set; }

        [JsonProperty("Torso")]
        private TemplateCacheKey _torso { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Torso is invalid.")]
        [Display(Name = "Torso Object", Description = "The # of and object that this thing's torso is made of.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
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

        [JsonIgnore]
        [NonNullableDataIntegrity("Head is invalid.")]
        [Display(Name = "Head Object", Description = "The object that this thing's head is made of.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
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

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        [Display(Name = "Extra Parts", Description = "The additional non-standard anatomical features this has. Tails, head fins, wings and unique forms (like eleven ears) qualify.")]
        [UIHint("BodyParts")]
        public HashSet<BodyPart> BodyParts { get; set; }

        /// <summary>
        /// Dietary type of this race
        /// </summary>
        [Display(Name = "Diet", Description = "What this can eat for nutritional purposes.")]
        [UIHint("EnumDropDownList")]
        public DietType DietaryNeeds { get; set; }

        [JsonProperty("SanguinaryMaterial")]
        private TemplateCacheKey _sanguinaryMaterial { get; set; }

        /// <summary>
        /// Material that is the blood
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Blood material is invalid.")]
        [Display(Name = "Blood Type", Description = "The material this thing's blood is composed of.")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
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
        [Range(0, 200)]
        [Display(Name = "Vision Range", Description = "The range of luminosity this can see clearly in.")]
        [UIHint("ValueRangeShort")]
        public ValueRange<short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        [Range(0, 200)]
        [Display(Name = "Heat Tolerence", Description = "The range of what temperature this can tolerate. Below this is 'too cold' and the thing will suffer ill effects.")]
        [UIHint("ValueRangeShort")]
        public ValueRange<short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        [Display(Name = "Breathes", Description = "What mediums this can breathe in.")]
        [UIHint("EnumDropDownList")]
        public RespiratoryType Breathes { get; set; }

        /// <summary>
        /// The type of damage biting inflicts
        /// </summary>
        [Display(Name = "Teeth", Description = "What style of teeth this thing has.")]
        [UIHint("EnumDropDownList")]
        public DamageType TeethType { get; set; }

        /// <summary>
        /// The name used to describe a large gathering of this race
        /// </summary>
        [StringDataIntegrity("Races must have a collective noun between 2 and 50 characters long.", 2, 50)]
        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Collective Noun", Description = "The 'herd name' for this race. Like 'herd' for deer and cows or 'pack' for wolves.")]
        [DataType(DataType.Text)]
        public string CollectiveNoun { get; set; }

        [JsonProperty("StartingLocation")]
        private TemplateCacheKey _startingLocation { get; set; }

        /// <summary>
        /// What is the starting room of new players
        /// </summary>

        [JsonIgnore]
        [NonNullableDataIntegrity("Starting Location is invalid.")]
        [Display(Name = "Starting Zone", Description = "The zone this begins in when made as a player.")]
        [UIHint("ZoneTemplateList")]
        [ZoneTemplateDataBinder]
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

        [JsonIgnore]
        [NonNullableDataIntegrity("Emergency Location is invalid.")]
        [Display(Name = "Recall Zone", Description = "The 'emergency' zone this shows up in when the system can't figure out where else to put it. (post-newbie zone for players)")]
        [UIHint("ZoneTemplateList")]
        [ZoneTemplateDataBinder]
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
        /// The body of the death notice
        /// </summary>
        [Display(Name = "Death Notice", Description = "The text that goes into the death notice per race.")]
        [UIHint("Markdown")]
        public MarkdownString DeathNoticeBody { get; set; }

        /// <summary>
        /// Should any qualities of the player change on death (like money removal)
        /// </summary>
        [Display(Name = "Death Changes", Description = "The qualities that are changed when someone dies. Typically physical losses.")]
        [UIHint("QualityValueList")]
        public HashSet<QualityValue> DeathQualityChanges { get; set; }

        /// <summary>
        /// Make a new blank race
        /// </summary>
        public Race()
        {
            BodyParts = new HashSet<BodyPart>();
            DeathQualityChanges = new HashSet<QualityValue>();
        }

        /// <summary>
        /// Method to get the full list of anatomical features of this race
        /// </summary>
        public IEnumerable<BodyPart> FullAnatomy()
        {
            List<BodyPart> anatomy = new();

            if (Arms != null)
            {
                int i = 1;
                while (i < Arms.Amount)
                {
                    anatomy.Add(new BodyPart(Arms, string.Format("Arm {0}", i.ToGreek(true))));
                    i++;
                }
            }

            if (Legs != null)
            {
                int i = 1;
                while (i < Arms.Amount)
                {
                    anatomy.Add(new BodyPart(Legs, string.Format("Leg {0}", i.ToGreek(true))));
                    i++;
                }
            }

            if (Head != null)
            {
                anatomy.Add(new BodyPart(new InanimateComponent(Head, 1), "Head"));
            }

            if (Torso != null)
            {
                anatomy.Add(new BodyPart(new InanimateComponent(Torso, 1), "Torso"));
            }

            foreach (BodyPart bit in BodyParts)
            {
                int i = 1;
                while (i < bit.Part.Amount)
                {
                    anatomy.Add(new BodyPart(bit.Part, bit.Name));
                    i++;
                }
            }

            return anatomy;
        }

        /// <summary>
        /// Render this race's body as an ascii.. thing
        /// </summary>
        /// <returns>List of strings as rows for rendering</returns>
        public IEnumerable<string> RenderAnatomy(bool forWeb)
        {
            List<string> stringList = new();

            if (Head != null)
            {
                stringList.Add(Head.Model.ModelTemplate.ViewFlattenedModel(forWeb));
            }

            if (Arms != null)
            {
                int armCount = 0;
                while(armCount < Arms.Amount)
                {
                    armCount++;
                    stringList.Add(Arms.Item.Model.ModelTemplate.ViewFlattenedModel(forWeb));
                }
            }

            if (Head != null)
            {
                stringList.Add(Torso.Model.ModelTemplate.ViewFlattenedModel(forWeb));
            }

            if (Legs != null)
            {
                int legCount = 0;
                while (legCount < Legs.Amount)
                {
                    legCount++;
                    stringList.Add(Legs.Item.Model.ModelTemplate.ViewFlattenedModel(forWeb));
                }
            }

            foreach (BodyPart bit in BodyParts)
            {
                if (bit.Part == null)
                {
                    continue;
                }

                for (int i = 0; i < bit.Part.Amount; i++)
                {
                    int legCount = 0;
                    while (legCount < Legs.Amount)
                    {
                        legCount++;
                        stringList.Add(bit.Part.Item.Model.ModelTemplate.ViewFlattenedModel(forWeb));
                    }
                }
            }

            return stringList;
        }

        /// <summary>
        /// Gets the errors for Template fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your Template is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> TemplateProblems = base.FitnessReport();

            //Gotta keep most of these in due to the tuple thing
            if (Arms == null || Arms.Item == null || Arms.Amount < 0)
            {
                TemplateProblems.Add("Arms are invalid.");
            }

            if (Legs == null || Legs.Item == null || Legs.Amount < 0)
            {
                TemplateProblems.Add("Legs are invalid.");
            }

            if (BodyParts != null && BodyParts.Any(a => a.Part == null || a.Part.Amount == 0 || string.IsNullOrWhiteSpace(a.Name)))
            {
                TemplateProblems.Add("BodyParts are invalid.");
            }

            if (VisionRange == null || VisionRange.Low >= VisionRange.High)
            {
                TemplateProblems.Add("Vision range is invalid.");
            }

            if (TemperatureTolerance == null || TemperatureTolerance.Low >= TemperatureTolerance.High)
            {
                TemplateProblems.Add("Temperature tolerance is invalid.");
            }

            return TemplateProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Collective", CollectiveNoun);
            returnList.Add("Starting Zone", StartingLocation.Name);
            returnList.Add("Recall Zone", EmergencyLocation.Name);

            returnList.Add("Head", Head.Name);
            returnList.Add("Torso", Torso.Name);
            returnList.Add("Legs", string.Format("{1} {0}", Legs.Item.Name, Legs.Amount));
            returnList.Add("Arms", string.Format("{1} {0}", Arms.Item.Name, Arms.Amount));
            returnList.Add("Blood", SanguinaryMaterial.Name);
            returnList.Add("Teeth", TeethType.ToString());
            returnList.Add("Breathes", Breathes.ToString());
            returnList.Add("Diet", DietaryNeeds.ToString());
            returnList.Add("Vision Range", string.Format("{0} - {1}", VisionRange.Low, VisionRange.High));
            returnList.Add("Temperature Tolerance", string.Format("{0} - {1}", TemperatureTolerance.Low, TemperatureTolerance.High));

            foreach (BodyPart part in BodyParts)
            {
                returnList.Add("Body Parts", string.Format("{0} - {1} ({2})", part.Part.Amount.ToString(), part.Part.Item.Name, part.Name));
            }

            return returnList;
        }
    }
}
