using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
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
        [Display(Name = "Name", Description = "The descriptive name of the race. Used a lot in displays.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
   
        [Display(Name = "Arm Object", Description = "The object that this thing's arms are made of.")]
        [DataType(DataType.Text)]
        public long ArmsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Arms", Description = "The number of arms this thing has. Humans have 2, dogs have ZERO. It doesn't have to have arms.")]
        [DataType(DataType.Text)]
        public short ArmsAmount { get; set; }
      
        [Display(Name = "Leg Object", Description = "The object that this thing's legs are made of.")]
        [DataType(DataType.Text)]
        public long LegsID { get; set; }

        [Range(0, 8)]
        [Display(Name = "# of Legs", Description = "The number of legs this thing has. Humans have 2, dogs have 4. It doesn't have to have legs at all.")]
        [DataType(DataType.Text)]
        public short LegsAmount { get; set; }
        
        [Display(Name = "Torso Object", Description = "The object that this thing's torso is made of.")]
        [DataType(DataType.Text)]
        public long TorsoId { get; set; }

        
        [Display(Name = "Head Object", Description = "The object that this thing's head is made of.")]
        [DataType(DataType.Text)]
        public long HeadId { get; set; }
       
        [Display(Name = "Extra Parts", Description = "The additional non-standard anatomical features this has. Tails, head fins, wings and unique forms (like eleven ears) qualify.")]
        public long[] ExtraPartsId { get; set; }

        [Display(Name = "Amount", Description = "The number of this extra part this race has.")]
        public short[] ExtraPartsAmount { get; set; }
        
        [Display(Name = "Name", Description = "The descriptive name of this extra part.")]
        public string[] ExtraPartsName { get; set; }
        
        [Display(Name = "Diet", Description = "What this can eat for nutritional purposes.")]
        [DataType(DataType.Text)]
        public short DietaryNeeds { get; set; }
        
        [Display(Name = "Blood Type", Description = "The material this thing's blood is composed of.")]
        [DataType(DataType.Text)]
        public long BloodId { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range Low", Description = "The low cap of luminosity this can see clearly in.")]
        [DataType(DataType.Text)]
        public short VisionRangeLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Vision Range High", Description = "The high cap of luminosity this can see clearly in.")]
        [DataType(DataType.Text)]
        public short VisionRangeHigh { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence Low", Description = "The low cap of what temperature this can tolerate. Below this is 'too cold' and the thing will suffer ill effects.")]
        [DataType(DataType.Text)]
        public short TemperatureToleranceLow { get; set; }

        [Range(0, 200)]
        [Display(Name = "Heat Tolerence High", Description = "The high cap of what temperature this can tolerate. Below this is 'too hot' and the thing will suffer ill effects.")]
        [DataType(DataType.Text)]
        public short TemperatureToleranceHigh { get; set; }
    
        [Display(Name = "Breathes", Description = "What mediums this can breathe in.")]
        [DataType(DataType.Text)]
        public short Breathes { get; set; }
        
        [Display(Name = "Teeth", Description = "What style of teeth this thing has.")]
        [DataType(DataType.Text)]
        public short TeethType { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Collective Noun", Description = "The 'herd name' for this race. Like 'herd' for deer and cows or 'pack' for wolves.")]
        [DataType(DataType.Text)]
        public string CollectiveNoun { get; set; }

        [Display(Name = "Starting Zone", Description = "The zone this begins in when made as a player.")]
        [DataType(DataType.Text)]
        public long StartingLocationId { get; set; }
        
        [Display(Name = "Recall Zone", Description = "The 'emergency' zone this shows up in when the system can't figure out where else to put it. (post-newbie zone for players)")]
        [DataType(DataType.Text)]
        public long RecallLocationId { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Help Text Body", Description = "The descriptive text shown in the public help pages and when HELP is used in game.")]
        public string HelpBody { get; set; }

        public IEnumerable<IZoneData> ValidZones { get; set; }
        public IEnumerable<IInanimateData> ValidObjects { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IRace DataObject { get; set; }
    }
}