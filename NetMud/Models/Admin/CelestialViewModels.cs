using NetMud.Authentication;
using NetMud.DataStructure.Base.World;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageCelestialsViewModel : PagedDataModel<ICelestial>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageCelestialsViewModel(IEnumerable<ICelestial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ICelestial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditCelestialViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditCelestialViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Given Name", Description = "First Name.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// Orbit Type
        /// </summary>
        [Display(Name = "Orbital Orientation", Description = "What type of orbit this has. Heliocentric means the world orbits this.")]
        short OrientationType { get; set; }

        [Display(Name = "Apogee", Description = "Maximal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        int Apogee { get; set; }

        [Display(Name = "Perigree", Description = "Minimal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        int Perigree { get; set; }

        [Display(Name = "Velocity", Description = "How fast is this hurtling through space. (affects a LOT of things)")]
        int Velocity { get; set; }

        [Display(Name = "Velocity", Description = "How bright is this. Measured in thousands. Anything less than 1000 is not visible to the naked eye.")]
        int Luminosity { get; set; }

        public ICelestial DataObject { get; set; }
    }
}