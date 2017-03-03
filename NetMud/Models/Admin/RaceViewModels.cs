using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageRaceDataViewModel : PagedDataModel<IRace>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRaceDataViewModel(IEnumerable<IRace> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRace, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditRaceViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditRaceViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidObjects = Enumerable.Empty<IInanimateData>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]  
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        
        [Display(Name = "Arm Object")]
        [DataType(DataType.Text)]
        public long ArmsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Arms")]
        [DataType(DataType.Text)]
        public short ArmsAmount { get; set; }

        
        [Display(Name = "Leg Object")]
        [DataType(DataType.Text)]
        public long LegsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Legs")]
        [DataType(DataType.Text)]
        public short LegsAmount { get; set; }

        
        [Display(Name = "Torso Object")]
        [DataType(DataType.Text)]
        public long TorsoId { get; set; }

        
        [Display(Name = "Head Object")]
        [DataType(DataType.Text)]
        public long HeadId { get; set; }

        
        [Display(Name = "Extra Parts")]
        public long[] ExtraPartsId { get; set; }

        
        [Display(Name = "Extra Parts")]
        public short[] ExtraPartsAmount { get; set; }

        
        [Display(Name = "Extra Parts")]
        public string[] ExtraPartsName { get; set; }

        
        [Display(Name = "Diet")]
        [DataType(DataType.Text)]
        public short DietaryNeeds { get; set; }

        
        [Display(Name = "Blood Type")]
        [DataType(DataType.Text)]
        public long BloodId { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range Low")]
        [DataType(DataType.Text)]
        public short VisionRangeLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range High")]
        [DataType(DataType.Text)]
        public short VisionRangeHigh { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence Low")]
        [DataType(DataType.Text)]
        public short TemperatureToleranceLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence High")]
        [DataType(DataType.Text)]
        public short TemperatureToleranceHigh { get; set; }

        
        [Display(Name = "Breathes")]
        [DataType(DataType.Text)]
        public short Breathes { get; set; }

        
        [Display(Name = "Teeth")]
        [DataType(DataType.Text)]
        public short TeethType { get; set; }

        
        [Display(Name = "Starting Room")]
        [DataType(DataType.Text)]
        public long StartingLocationId { get; set; }

        
        [Display(Name = "Recall Room")]
        [DataType(DataType.Text)]
        public long RecallLocationId { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string HelpBody { get; set; }

        public IEnumerable<IInanimateData> ValidObjects { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IRace DataObject { get; set; }
    }
}