using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.Models.Admin
{
    public class ReferenceDataDropdownModel
    {
        public ReferenceDataDropdownModel(string controlName, string label, IEnumerable<IKeyedData> validItemList, long selectedItemId)
        {
            ControlName = controlName;
            Label = label;
            ValidList = validItemList;
            SelectedItemId = selectedItemId;
        }

        public string ControlName { get; set; }
        public string Label { get; set; }
        public IEnumerable<IKeyedData> ValidList { get; set; }
        public long SelectedItemId { get; set; }
    }
}