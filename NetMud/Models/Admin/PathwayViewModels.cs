using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class AddEditPathwayDataViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditPathwayDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [Range(0, 16, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Visible message strength")]
        public int VisibleStrength { get; set; }

        [Range(0, 16, ErrorMessage = "The {0} must be between {2} and {1}.")] 
        [Display(Name = "Audible message strength")]
        public int AudibleStrength { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Visible message to area")]
        public string VisibleToSurroundings { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]       
        [Display(Name = "Audible message to area")]
        public string AudibleToSurroundings { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        
        [Display(Name = "Message to Destination")]
        public string MessageToDestination { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]     
        [Display(Name = "Message to Origin")]
        public string MessageToOrigin { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]      
        [Display(Name = "Message to Actor")]
        public string MessageToActor { get; set; }
        
        [Display(Name = "To Room")]
        public long ToLocationID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North")]
        public int DegreesFromNorth { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData ToLocation { get; set; }
        public IPathwayData DataObject { get; set; }
    }
}