using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Inanimate;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Inanimates
{
    [Serializable]
    public class InanimateComponent : IInanimateComponent
    {
        [JsonProperty("Item")]
        public TemplateCacheKey _item { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public IInanimateTemplate Item
        {
            get
            {
                if (_item == null)
                    return null;

                return TemplateCache.Get<IInanimateTemplate>(_item);
            }
            set
            {
                _item = new TemplateCacheKey(value);
            }
        }

        public int Amount { get; set; }

        public InanimateComponent()
        {
            Amount = 0;
        }

        public InanimateComponent(IInanimateTemplate item, int amount)
        {
            Amount = amount;
            Item = item;
        }
    }
}
