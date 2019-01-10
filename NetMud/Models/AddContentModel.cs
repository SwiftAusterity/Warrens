using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.Models
{
    public abstract class AddContentModel<T> where T : IKeyedData
    {
        public T DataTemplate { get; set; }
        public IEnumerable<T> ValidTemplateBases { get; set; }

        public abstract T Template { get; set; }

        public AddContentModel(long templateId)
        {
            DataTemplate = TemplateCache.Get<T>(templateId);
            ValidTemplateBases = TemplateCache.GetAll<T>(true);
        }
    }
}