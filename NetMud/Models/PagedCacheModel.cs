using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models
{
    public abstract class PagedCacheModel<T> : IPagedDataModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public string ModelEntityType { get; set; }

        [Range(1, 10000, ErrorMessage = "Page number must be at least 1.")]
        [RegularExpression("[0-9]+")]
        [Display(Name = "Page Number", Description = "Your current page number.")]
        public int CurrentPageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Items per page must be between {1} and {2}.")]
        [RegularExpression("[0-9]+")]
        [Display(Name = "Items Per Page", Description = "How many items to display per page.")]
        public int ItemsPerPage { get; set; }

        [StringLength(2000, ErrorMessage = "Search Terms must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "Search", Description = "Filter by keywords and names.")]
        public string SearchTerms { get; set; }

        [Display(Name = "Item", Description = "Valid items for the display list.")]
        public FilteredQuery<T> DataQuery { get; private set; }

        internal abstract Func<T, bool> SearchFilter { get; }
        internal abstract Func<T, object> OrderPrimary { get; }
        internal abstract Func<T, object> OrderSecondary { get; }

        public PagedCacheModel(CacheType dataType)
        {
            DataQuery = new FilteredQuery<T>(dataType);
        }

        public IEnumerable<T> CurrentPageOfItems
        {
            get
            {
                RefreshDataQueryParameters();

                return DataQuery.ExecuteQuery();
            }
        }

        public int NumberOfPages
        {
            get
            {
                RefreshDataQueryParameters();

                return (int)Math.Ceiling(Math.Max(1, DataQuery.FilteredCount() / (double)ItemsPerPage));
            }
        }

        private void RefreshDataQueryParameters()
        {
            DataQuery.ItemsPerPage = ItemsPerPage;
            DataQuery.CurrentPageNumber = CurrentPageNumber;
            DataQuery.Filter = SearchFilter;
            DataQuery.OrderPrimary = OrderPrimary;
            DataQuery.OrderSecondary = OrderSecondary;
        }
    }
}