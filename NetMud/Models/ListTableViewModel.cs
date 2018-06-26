using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Models
{
    public class ListTableViewModel
    {
        public IEnumerable<dynamic> Values { get; set; }
        public Dictionary<string, Func<dynamic, string>> Columns { get; set; }
        public bool IncludeDelete { get; set; }
        public bool IncludeUnapprove { get; set; }
        public string Name { get; set; }

        public ListTableViewModel()
        {
            Values = Enumerable.Empty<object>();
            Columns = new Dictionary<string, Func<dynamic, string>>();
            IncludeDelete = true;
            IncludeUnapprove = true;
        }
    }
}