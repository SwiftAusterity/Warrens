using NetMud.Authentication;
using NetMud.DataStructure.Base.World;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageGaiaViewModel : PagedDataModel<IGaiaData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageGaiaViewModel(IEnumerable<IGaiaData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGaiaData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditGaiaViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditGaiaViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The name of the world.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Celestial Body", Description = "The Celestial bodies that orbit this world. (or the one this orbits)")]
        public long[] CelestialBodies { get; set; }

        [Display(Name = "Month Name", Description = "A name of the month, in order.")]
        [DataType(DataType.Text)]
        public string MonthNames { get; set; }

        [Display(Name = "Days per Month", Description = "How many days there are per month.")]
        [DataType(DataType.Text)]
        public int DaysPerMonth { get; set; }

        [Display(Name = "Hours per Day", Description = "How many hours there are per day.")]
        [DataType(DataType.Text)]
        public int HoursPerDay { get; set; }

        [Display(Name = "Reset Year", Description = "What year this world resets to if made from nothing.")]
        [DataType(DataType.Text)]
        public int StartingYear { get; set; }

        public IEnumerable<ICelestial> ValidCelestials { get; set; }
        public IGaiaData DataObject { get; set; }
    }
}