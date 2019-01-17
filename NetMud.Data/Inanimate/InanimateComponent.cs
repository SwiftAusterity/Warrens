using NetMud.Data.Architectural.PropertyBinding;
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
        [Display(Name = "Component", Description = "The object of the collection.")]
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
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

        [Display(Name = "Amount", Description = "Amount of the component item in the collection.")]
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
