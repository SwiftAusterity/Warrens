using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Models
{
    public class ListTableViewModel
    {
        public string Name { get; set; }
        public IEnumerable<dynamic> Values { get; set; }
        public Dictionary<string, Func<dynamic, string>> Columns { get; set; }

        public IPagedDataModel PagingModel { get; set; }
        public string EntityTypeName { get; set; }
        public string AddMethodName { get; set; }
        public string RemoveMethodName { get; set; }

        public bool IncludeDelete { get; set; }
        public bool IncludeUnapprove { get; set; }
        public bool IncludeBasicPagingandSearch { get; set; }
        public bool IncludeAddLink { get; set; }

        public ListTableViewModel()
        {
            Values = Enumerable.Empty<object>();
            Columns = new Dictionary<string, Func<dynamic, string>>();
            IncludeDelete = true;
            IncludeUnapprove = true;
            IncludeBasicPagingandSearch = true;
            IncludeAddLink = true;
        }
    }
}