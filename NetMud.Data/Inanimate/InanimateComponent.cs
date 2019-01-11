using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Inanimate;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Inanimate
{
    [Serializable]
    public class InanimateComponent : IInanimateComponent
    {
        [JsonProperty("Item")]
        public TemplateCacheKey _item { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Component", Description = "A required component of the crafting recipe.")]
        [UIHint("InanimateTemplateList")]
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

        [Display(Name = "Needed", Description = "Amount of the component item needed.")]
        [DataType(DataType.Text)]
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
