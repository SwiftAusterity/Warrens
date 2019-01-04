using NetMud.Data.Architectural;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Tile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Tiles
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    [Serializable]
    public class TileTemplate : TemplatePartial, ITileTemplate
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Leader; } }

        [JsonProperty("OwnerWorld")]
        public TemplateCacheKey _ownerWorld { get; set; }

        /// <summary>
        /// The world this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IGaiaTemplate OwnerWorld
        {
            get
            {
                if (_ownerWorld == null)
                    return null;

                return TemplateCache.Get<IGaiaTemplate>(_ownerWorld);
            }
            set
            {
                if (value == null)
                    return;

                _ownerWorld = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("AsciiCharacter")]
        internal byte[] _asciiCharacter { get; set; }

        /// <summary>
        /// The character displayed on the visual map
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [UnicodeCharacterValidator(ErrorMessage = "Single characters and single emoticons only.", Optional = true)]
        [Display(Name = "Ascii Character", Description = "The character displayed on the visual map.")]
        [DataType(DataType.Text)]
        public virtual string AsciiCharacter
        {
            get
            {
                return Encoding.UTF32.GetString(_asciiCharacter);
            }
            set
            {
                if (value == null)
                    _asciiCharacter = Encoding.UTF32.GetBytes("");
                else
                    _asciiCharacter = Encoding.UTF32.GetBytes(value);
            }
        }

        /// <summary>
        /// The hex code of the color of the ascii character
        /// </summary>
        [Display(Name = "Color", Description = "The hex code of the color of the ascii character.")]
        [DataType(DataType.Text)]
        [UIHint("ColorPicker")]
        [Required]
        public string HexColorCode { get; set; }

        /// <summary>
        /// The background color for the display
        /// </summary>
        [Display(Name = "Background Color", Description = "The hex code of the color of the background of the tile.")]
        [DataType(DataType.Text)]
        [UIHint("ColorPicker")]
        [Required]
        public string BackgroundHexColor { get; set; }

        /// <summary>
        /// The long description of the entity
        /// </summary>
        [StringLength(1000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Description", Description = "Longer description for this tile.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        /// Can be walked on
        /// </summary>
        [Display(Name = "Pathable", Description = "Can you walk on this.")]
        [UIHint("Boolean")]
        [Required]
        public bool Pathable { get; set; }

        /// <summary>
        /// Can be swam in
        /// </summary>
        [Display(Name = "Aquatic", Description = "Can you swim in this.")]
        [UIHint("Boolean")]
        [Required]
        public bool Aquatic { get; set; }

        /// <summary>
        /// Can be flown through?
        /// </summary>
        [Display(Name = "Air", Description = "Can you fly on this.")]
        [UIHint("Boolean")]
        [Required]
        public bool Air { get; set; }

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        public HashSet<IInteraction> Interactions { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        public HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public TileTemplate()
        {
            DecayEvents = new HashSet<IDecayEvent>();
            Interactions = new HashSet<IInteraction>();
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
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Ascii Character", AsciiCharacter);
            returnList.Add("Color", HexColorCode);
            returnList.Add("Background", BackgroundHexColor);

            returnList.Add("Pathable", Pathable.ToString());
            returnList.Add("Aquatic", Aquatic.ToString());
            returnList.Add("Air", Air.ToString());

            returnList.Add("Description", Description);

            return returnList;
        }

        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            //approval will be handled inside the base call
            IKeyedData obj = base.Create(creator, rank);

            return obj;
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
            HashSet<IDecayEvent> decayEvents = new HashSet<IDecayEvent>();
            foreach (IDecayEvent decayEvent in DecayEvents)
                decayEvents.Add((IDecayEvent)decayEvent.Clone());

            HashSet<IInteraction> interactions = new HashSet<IInteraction>();
            foreach (IInteraction interaction in Interactions)
                interactions.Add((IInteraction)interaction.Clone());

            return new TileTemplate
            {
                Name = Name,
                AsciiCharacter = AsciiCharacter,
                Description = Description,
                HexColorCode = HexColorCode,
                Interactions = Interactions,
                DecayEvents = decayEvents,
                BackgroundHexColor = BackgroundHexColor,
                Aquatic = Aquatic,
                Air = Air,
                Pathable = Pathable
            };
        }
    }
}
