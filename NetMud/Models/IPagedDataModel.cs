using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
    public interface IPagedDataModel
    {
        [Display(Name = "Current Page", Description = "The current page you are on")]
        [DataType(DataType.Text)]
        int CurrentPageNumber { get; set; }

        [Display(Name = "Per Page", Description = "How many items per page will be displayed")]
        [DataType(DataType.Text)]
        int ItemsPerPage { get; set; }

        [Display(Name = "Search By", Description = "Filter by term")]
        [DataType(DataType.Text)]
        string SearchTerms { get; set; }

        [Display(Name = "Pages", Description = "How many total pages there are")]
        [DataType(DataType.Text)]
        int NumberOfPages { get; }
    }
}