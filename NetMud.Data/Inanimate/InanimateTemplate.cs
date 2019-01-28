using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Inanimate
{
    /// <summary>
    /// Backing data for Inanimate objects
    /// </summary>
    [Serializable]
    public class InanimateTemplate : EntityTemplatePartial, IInanimateTemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Inanimate); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

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
                    List<string> wordList = new List<string>() { Name };
                    wordList.AddRange(Name.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    _keywords = wordList.ToArray();
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// Definition for the room's capacity for mobiles
        /// </summary>
        [UIHint("MobileEntityContainers")]
        public HashSet<IEntityContainerData<IMobile>> MobileContainers { get; set; }

        /// <summary>
        /// Definition for the room's capacity for inanimates
        /// </summary>
        [UIHint("InanimateEntityContainers")]
        public HashSet<IEntityContainerData<IInanimate>> InanimateContainers { get; set; }

        /// <summary>
        /// How many of this can be in a stack
        /// </summary>
        [Display(Name = "Accumulation Cap", Description = "How many of this can go in one 'stack'.")]
        [Range(0, 999, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Required]
        public int AccumulationCap { get; set; }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical model is invalid.")]
        [UIHint("TwoDimensionalModel")]
        public IDimensionalModel Model { get; set; }

        #region Crafting
        /// <summary>
        /// The amount produced
        /// </summary>
        [Display(Name = "Produces", Description = "How many of this thing does a single craft produce.")]
        [DataType(DataType.Text)]
        public int Produces { get; set; }

        /// <summary>
        /// Object type + amount needed to craft this
        /// </summary>
        [UIHint("InanimateComponents")]
        public IEnumerable<IInanimateComponent> Components { get; set; }

        /// <summary>
        /// List of quality/value pairs needed by the crafter
        /// </summary>
        [UIHint("QualityValueList")]
        public HashSet<QualityValue> SkillRequirements { get; set; }

        /// <summary>
        /// Make the thing
        /// </summary>
        /// <returns>blank or error message</returns>
        public string Craft(IEntity crafter)
        {
            string returnValue = string.Empty;

            if (!(crafter is IContains crafterContainer))
            {
                return "Crafter is invalid.";
            }

            if (Produces <= 0 || Components.Count() <= 0)
            {
                return "This item can not be crafted.";
            }

            IEnumerable<IInanimate> crafterInventory = crafterContainer.GetContents<IInanimate>();

            //Find components
            List<IInanimate> itemsToUse = new List<IInanimate>();
            foreach (IInanimateComponent component in Components)
            {
                int inventoryCount = crafterInventory.Count(item => item.TemplateId.Equals(component.Item.Id));

                if (inventoryCount < component.Amount)
                {
                    return string.Format("You need at least {0} {1}s to craft {2}.", component.Amount, component.Item.Name, Name);
                }

                itemsToUse.AddRange(crafterInventory.Where(item => item.TemplateId.Equals(component.Item.Id))
                                                    .Take(component.Amount));
            }

            //Remove the components
            foreach (IInanimate item in itemsToUse)
            {
                item.Remove();
            }

            int i = 0;
            while (i < Produces)
            {
                Inanimate newItem = new Inanimate(this, crafterContainer.GetContainerAsLocation());
                i++;
            }

            return returnValue;
        }

        /// <summary>
        /// Render the blueprints to someone
        /// </summary>
        /// <returns>formatted list of components and requirements</returns>
        public string RenderBlueprints(IEntity actor)
        {
            StringBuilder returnValue = new StringBuilder();

            returnValue.AppendFormattedLine("Crafts: ({0}) {1}", Produces, Name);

            returnValue.AppendLine("Components");
            foreach (IInanimateComponent component in Components.Where(item => item.Item != null && item.Amount > 0))
            {
                returnValue.AppendFormattedLine("({0}) {1}{2}", component.Amount, component.Item, component.Amount > 1 ? "s" : "");
            }

            returnValue.AppendLine("Required Skills");
            foreach (QualityValue component in SkillRequirements.Where(item => !string.IsNullOrWhiteSpace(item.Quality) && item.Value > 0))
            {
                returnValue.AppendFormattedLine("{0}:{1}", component.Quality, component.Value);
            }

            return returnValue.ToString();
        }
        #endregion

        /// <summary>
        /// Spawns a new empty inanimate object
        /// </summary>
        public InanimateTemplate()
        {
            InanimateContainers = new HashSet<IEntityContainerData<IInanimate>>();
            MobileContainers = new HashSet<IEntityContainerData<IMobile>>();
            Components = new HashSet<IInanimateComponent>();
            Qualities = new HashSet<IQuality>();
            SkillRequirements = new HashSet<QualityValue>();
            Model = new DimensionalModel();
            Descriptives = new HashSet<ISensoryEvent>();
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
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Accumulation Cap", AccumulationCap.ToString());

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

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new InanimateTemplate
            {
                Name = Name,
                Qualities = Qualities,
                SkillRequirements = SkillRequirements,
                Produces = Produces,
                InanimateContainers = InanimateContainers,
                Components = Components,
                AccumulationCap = AccumulationCap
            };
        }
    }
}
