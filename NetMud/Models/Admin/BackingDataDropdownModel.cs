using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetMud.Models.Admin
{
    public class BackingDataDropdownModel
    {
        public BackingDataDropdownModel(string controlName, string label, IEnumerable<IEntityBackingData> validItemList, long selectedItemId)
        {
            ControlName = controlName;
            Label = label;
            ValidList = validItemList;
            SelectedItemId = selectedItemId;
        }

        public string ControlName { get; set; }
        public string Label { get; set; }
        public IEnumerable<IEntityBackingData> ValidList { get; set; }
        public long SelectedItemId { get; set; }
    }
}