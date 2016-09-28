using NetMud.Authentication;
using NetMud.Data.Reference;
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
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Arm Object")]
        public long NewArmsID { get; set; }

        [Range(0, 8)]
        [DataType(DataType.Text)]
        [Display(Name = "# of Arms")]
        public short NewArmsAmount { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Leg Object")]
        public long NewLegsID { get; set; }

        [Range(0, 8)]
        [DataType(DataType.Text)]
        [Display(Name = "# of Legs")]
        public short NewLegsAmount { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Torso Object")]
        public long NewTorsoId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Head Object")]
        public long NewHeadId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Extra Parts")]
        public long[] NewExtraPartsId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Extra Parts")]
        public short[] NewExtraPartsAmount { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Extra Parts")]
        public string[] NewExtraPartsName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Diet")]
        public short NewDietaryNeeds { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Blood Type")]
        public long NewBloodId { get; set; }

        [Range(0, 200)]
        [DataType(DataType.Text)]
        [Display(Name = "Vision Range")]
        public short NewVisionRangeLow { get; set; }

        [Range(0, 200)]
        [DataType(DataType.Text)]
        [Display(Name = "Vision Range High")]
        public short NewVisionRangeHigh { get; set; }

        [Range(0, 200)]
        [DataType(DataType.Text)]
        [Display(Name = "Heat Tolerence")]
        public short NewTemperatureToleranceLow { get; set; }

        [Range(0, 200)]
        [DataType(DataType.Text)]
        [Display(Name = "Heat Tolerence High")]
        public short NewTemperatureToleranceHigh { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Breathes")]
        public short NewBreathes { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Teeth")]
        public short NewTeethType { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Starting Room")]
        public long NewStartingLocationId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Recall Room")]
        public long NewRecallLocationId { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IEnumerable<IInanimateData> ValidObjects { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public Race DataObject { get; set; }
    }
}