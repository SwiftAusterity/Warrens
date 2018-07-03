namespace NetMud.Models
{
    public interface IPagedDataModel
    {
        int CurrentPageNumber { get; set; }

        int ItemsPerPage { get; set; }

        string SearchTerms { get; set; }

        int NumberOfPages { get; }
    }
}