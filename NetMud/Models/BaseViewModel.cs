using NetMud.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models
{
    public interface BaseViewModel
    {
        ApplicationUser authedUser { get; set; }
    }

    public abstract class PagedDataModel<T> : IPagedDataModel
    {
        public PagedDataModel(IEnumerable<T> items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            Items = items;
        }

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

        public IEnumerable<T> Items { get; private set; }

        internal abstract Func<T, bool> SearchFilter { get; }

        public IEnumerable<T> CurrentPageOfItems 
        {
            get
            {
                if(Items == null || Items.Count() == 0)
                    return Enumerable.Empty<T>();

                var skip = (CurrentPageNumber - 1) * ItemsPerPage;
                var take = Math.Abs(Items.Count() - skip) >= ItemsPerPage ? ItemsPerPage : Math.Abs(Items.Count() - skip);

                if(!string.IsNullOrWhiteSpace(SearchTerms))
                    return Items.Where(item => SearchFilter(item)).Skip(skip).Take(take);

                return Items.Skip(skip).Take(take);
            }
        }

        public int NumberOfPages
        {
            get
            {
                return (int)Math.Ceiling(Math.Min((double)1, Items.Count() / ItemsPerPage));
            }
        }
    }

    public interface IPagedDataModel
    {
        int CurrentPageNumber { get; set; }

        int ItemsPerPage { get; set; }

        string SearchTerms { get; set; }

        int NumberOfPages { get; }
    }
}