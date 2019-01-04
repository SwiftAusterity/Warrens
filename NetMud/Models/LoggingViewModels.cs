using NetMud.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Logging
{
    public class DashboardViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<string> ChannelNames { get; set; }
        public string SelectedLogContent { get; set; }

        
        [Display(Name = "Selected Channel:", Description = "Logs channels are named by purpose and function.")]
        public string SelectedLog { get; set; }
    }
}