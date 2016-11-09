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
    public class ManageRaceDataViewModel : PagedDataModel<Race>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRaceDataViewModel(IEnumerable<Race> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<Race, bool> SearchFilter
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
        public string NewName { get; set; }

        
        [Display(Name = "Arm Object")]
        [DataType(DataType.Text)]
        public long NewArmsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Arms")]
        [DataType(DataType.Text)]
        public short NewArmsAmount { get; set; }

        
        [Display(Name = "Leg Object")]
        [DataType(DataType.Text)]
        public long NewLegsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Legs")]
        [DataType(DataType.Text)]
        public short NewLegsAmount { get; set; }

        
        [Display(Name = "Torso Object")]
        [DataType(DataType.Text)]
        public long NewTorsoId { get; set; }

        
        [Display(Name = "Head Object")]
        [DataType(DataType.Text)]
        public long NewHeadId { get; set; }

        
        [Display(Name = "Extra Parts")]
        public long[] NewExtraPartsId { get; set; }

        
        [Display(Name = "Extra Parts")]
        public short[] NewExtraPartsAmount { get; set; }

        
        [Display(Name = "Extra Parts")]
        public string[] NewExtraPartsName { get; set; }

        
        [Display(Name = "Diet")]
        [DataType(DataType.Text)]
        public short NewDietaryNeeds { get; set; }

        
        [Display(Name = "Blood Type")]
        [DataType(DataType.Text)]
        public long NewBloodId { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range Low")]
        [DataType(DataType.Text)]
        public short NewVisionRangeLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range High")]
        [DataType(DataType.Text)]
        public short NewVisionRangeHigh { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence Low")]
        [DataType(DataType.Text)]
        public short NewTemperatureToleranceLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence High")]
        [DataType(DataType.Text)]
        public short NewTemperatureToleranceHigh { get; set; }

        
        [Display(Name = "Breathes")]
        [DataType(DataType.Text)]
        public short NewBreathes { get; set; }

        
        [Display(Name = "Teeth")]
        [DataType(DataType.Text)]
        public short NewTeethType { get; set; }

        
        [Display(Name = "Starting Room")]
        [DataType(DataType.Text)]
        public long NewStartingLocationId { get; set; }

        
        [Display(Name = "Recall Room")]
        [DataType(DataType.Text)]
        public long NewRecallLocationId { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body")]
        public string NewHelpBody { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IEnumerable<IInanimateData> ValidObjects { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IRace DataObject { get; set; }
    }
}