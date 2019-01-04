using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Base class for backing data
    /// </summary>
    [Serializable]
    public abstract class EntityTemplatePartial : TemplatePartial, ITemplate
    {
        /// <summary>
        /// The system type for the entity this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public abstract Type EntityClass { get; }

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

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        internal string[] _keywords;

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public abstract string[] Keywords { get; set; }

        [JsonProperty("AsciiCharacter")]
        internal byte[] _asciiCharacter { get; set; }

        /// <summary>
        /// The character displayed on the visual map
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        //[UnicodeCharacterValidator(ErrorMessage = "Single characters and single emoticons only.")]
        [Display(Name = "Ascii Character", Description = "The character displayed on the visual map.")]
        [DataType(DataType.Text)]
        [Required]
        public virtual string AsciiCharacter
        {
            get
            {
                if (_asciiCharacter == null)
                    return string.Empty;

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
        public virtual string HexColorCode { get; set; }

        /// <summary>
        /// The long description of the entity
        /// </summary>
        [StringLength(1000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Description", Description = "Longer description for this tile.")]
        [DataType(DataType.MultilineText)]
        public virtual string Description { get; set; }

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        public HashSet<IInteraction> Interactions { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        public HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// List of live qualities of this entity
        /// </summary>
        public HashSet<IQuality> Qualities { get; set; }

        public EntityTemplatePartial()
        {
            //empty instance for getting the dataTableName
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
        }

        /// <summary>
        /// Check for a quality
        /// </summary>
        /// <param name="name">Gets the value of the request quality</param>
        /// <returns>The value</returns>
        public virtual int GetQuality(string name)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
                return 0;

            return currentQuality.Value;
        }

        /// <summary>
        /// Add a quality (can be negative)
        /// </summary>
        /// <param name="value">The value you're adding</param>
        /// <param name="additive">Is this additive or replace-ive</param>
        /// <returns>The new value</returns>
        public int SetQuality(int value, string quality, bool additive = false)
        {
            IQuality currentQuality = Qualities.FirstOrDefault(qual => qual.Name.Equals(quality, StringComparison.InvariantCultureIgnoreCase));

            if (currentQuality == null)
            {
                Qualities.Add(new Quality()
                {
                    Name = quality,
                    Type = QualityType.Aspect,
                    Visible = true,
                    Value = value
                });

                return value;
            }

            if (additive)
                currentQuality.Value += value;
            else
                currentQuality.Value = value;

            return value;
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            if (EntityClass == null || EntityClass.GetInterface("IEntity", true) == null)
                dataProblems.Add("Entity Class type reference is broken.");

            return dataProblems;
        }

        public override object Clone()
        {
            throw new NotImplementedException("Not much point cloning generics.");
        }
    }
}
