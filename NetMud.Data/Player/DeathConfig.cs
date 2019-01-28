using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// Config to handle player death
    /// </summary>
    [Serializable]
    public class DeathConfig : IDeathConfig
    {
        [JsonProperty("DeathRecallZone")]
        public TemplateCacheKey _deathRecallZone { get; set; }

        /// <summary>
        /// The zone you return to on death
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Death Recall", Description = "The zone you return to on death.")]
        [UIHint("BackingDataDropdown")]
        [Required]
        [ZoneTemplateDataBinder]
        public IZoneTemplate DeathRecallZone
        {
            get
            {
                if (_deathRecallZone == null)
                {
                    return null;
                }

                return TemplateCache.Get<IZoneTemplate>(_deathRecallZone);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _deathRecallZone = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// The coordinates you recall to on death
        /// </summary>
        [Display(Name = "Death Coordinate X", Description = "The coordinates you recall to on death.")]
        [DataType(DataType.Text)]
        [Required]
        public Coordinate DeathRecallCoordinates { get; set; }

        /// <summary>
        /// The subject of the death notice notification sent on death
        /// </summary>
        [Display(Name = "Subject", Description = "The subject of the death notice notification sent on death.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeSubject { get; set; }

        /// <summary>
        /// The from field for the death notice
        /// </summary>
        [Display(Name = "From", Description = "The from field for the death notice.")]
        [DataType(DataType.Text)]
        [Required]
        public string DeathNoticeFrom { get; set; }

        /// <summary>
        /// The body of the death notice
        /// </summary>
        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType("Markdown")]
        [Display(Name = "Body", Description = "The body of the death notice.")]
        [Required]
        [MarkdownBinder]
        public MarkdownString DeathNoticeBody { get; set; }

        /// <summary>
        /// Should any qualities of the player change on death (like money removal)
        /// </summary>
        [Display(Name = "Quality", Description = " Should any qualities of the player change on death (like money removal).")]
        [DataType(DataType.Text)]
        public HashSet<QualityValue> QualityChanges { get; set; }

        public DeathConfig()
        {
            QualityChanges = new HashSet<QualityValue>();
            DeathRecallCoordinates = new Coordinate(0, 0, 0);
            DeathNoticeBody = string.Empty;
        }
    }
}
