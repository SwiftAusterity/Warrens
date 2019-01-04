using NetMud.DataAccess.Cache;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    [Serializable]
    public class NPCRepop : INPCRepop
    {
        public short Amount { get; set; }

        [JsonProperty("NPC")]
        private TemplateCacheKey _npc { get; set; }

        /// <summary>
        /// Collection of items that should spawn on new create/fallback create
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public INonPlayerCharacterTemplate NPC
        {
            get
            {
                if (_npc == null)
                {
                    return null;
                }

                return TemplateCache.Get<INonPlayerCharacterTemplate>(_npc);
            }
            set
            {
                if (value == null)
                    return;

                _npc = new TemplateCacheKey(value);
            }
        }

        public NPCRepop()
        {
            Amount = 0;
        }

        public NPCRepop(INonPlayerCharacterTemplate npc, short amount)
        {
            NPC = npc;
            Amount = amount;
        }
    }
}
