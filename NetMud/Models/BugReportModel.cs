using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
    public class BugReportModel
    {
        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 5)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Describe Your Bug")]
        public string Body { get; set; }
    }
}