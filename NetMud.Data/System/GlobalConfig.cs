using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    [Serializable]
    public class GlobalConfig : ConfigData, IGlobalConfig
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.GameWorld;

        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        [Display(Name = "Websocket Portal Available", Description = "Are new connections being accepted over websockets?")]
        [UIHint("Boolean")]
        public bool WebsocketPortalActive { get; set; }

        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        /// <summary>
        /// Is live translation active?
        /// </summary>
        [Display(Name = "Live Translation", Description = "Do new Dictata get translated to the UI languages?")]
        [UIHint("Boolean")]
        public bool TranslationActive { get; set; }

        /// <summary>
        /// The API key for your azure translation service
        /// </summary>
        [Display(Name = "Azure API Key", Description = "The API key for your azure translation service.")]
        [DataType(DataType.Text)]
        public string AzureTranslationKey { get; set; }

        /// <summary>
        /// Is the deep lex active?
        /// </summary>
        [Display(Name = "Deep Lex", Description = "Do words get deep lexed through Mirriam Webster?")]
        [UIHint("Boolean")]
        public bool DeepLexActive { get; set; }

        /// <summary>
        /// Dictionary key for the deep lex
        /// </summary>
        [Display(Name = "Mirriam Dictionary Key", Description = "The API key for your mirriam webster dictionary service.")]
        [DataType(DataType.Text)]
        public string MirriamDictionaryKey { get; set; }

        /// <summary>
        /// Thesaurus key for the deep lex
        /// </summary>
        [Display(Name = "Mirriam Thesaurus Key", Description = "The API key for your mirriam webster thesaurus service.")]
        [DataType(DataType.Text)]
        public string MirriamThesaurusKey { get; set; }


        public GlobalConfig()
        {
            Name = "LiveSettings";
            WebsocketPortalActive = true;
            AdminsOnly = false;
            UserCreationActive = true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GlobalConfig
            {
                Name = Name,
                WebsocketPortalActive = WebsocketPortalActive,
                UserCreationActive = UserCreationActive,
                AdminsOnly = AdminsOnly,
            };
        }
    }
}
