using NetMud.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Logging
{
    public class DashboardViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<string> ChannelNames { get; set; }
        public string SelectedLogContent { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Selected Channel:")]
        public string SelectedLog { get; set; }
    }
}