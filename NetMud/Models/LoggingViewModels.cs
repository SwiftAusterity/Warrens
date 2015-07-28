using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NetMud.Models.Logging
{
    public class DashboardViewModel : BaseViewModel
    {
        public IEnumerable<string> ChannelNames { get; set; }
        public string SelectedLogContent { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Selected Channel")]
        public string SelectedLog { get; set; }
    }
}