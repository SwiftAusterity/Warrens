using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public IZoneTemplate DeathRecallZone
        {
            get
            {
                if (_deathRecallZone == null)
                    return null;

                return TemplateCache.Get<IZoneTemplate>(_deathRecallZone);
            }
            set
            {
                if (value == null)
                    return;

                _deathRecallZone = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// The coordinates you recall to on death
        /// </summary>
        public Coordinate DeathRecallCoordinates { get; set; }

        /// <summary>
        /// The subject of the death notice notification sent on death
        /// </summary>
        public string DeathNoticeSubject { get; set; }

        /// <summary>
        /// The from field for the death notice
        /// </summary>
        public string DeathNoticeFrom { get; set; }

        /// <summary>
        /// The body of the death notice
        /// </summary>
        public MarkdownString DeathNoticeBody { get; set; }

        /// <summary>
        /// Should any qualities of the player change on death (like money removal)
        /// </summary>
        public HashSet<QualityValue> QualityChanges { get; set; }

        public DeathConfig()
        {
            QualityChanges = new HashSet<QualityValue>();
            DeathRecallCoordinates = new Coordinate(0, 0, 0);
            DeathNoticeBody = string.Empty;
        }
    }
}
