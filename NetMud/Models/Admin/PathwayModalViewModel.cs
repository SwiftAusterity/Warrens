using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Utility;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class PathwayModalViewModel : TwoDimensionalEntityEditViewModel
    {
        public PathwayModalViewModel(long currentPath, long originId, long destinationId)
        {
            ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            ValidRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => !rm.ID.Equals(originId) && !rm.ID.Equals(destinationId));

            if (destinationId > -1)
                ToLocation = BackingDataCache.Get<IRoomData>(destinationId);

            if (originId > -1)
                FromLocation = BackingDataCache.Get<IRoomData>(originId);

            if(currentPath > -1)
            {
                var obj = BackingDataCache.Get<IPathwayData>(currentPath);

                if (obj == null)
                    return;

                DataObject = obj;
                NewName = obj.Name;

                AudibleStrength = obj.AudibleStrength;
                AudibleToSurroundings = obj.AudibleToSurroundings;
                DegreesFromNorth = obj.DegreesFromNorth;
                MessageToActor = obj.MessageToActor;
                MessageToDestination = obj.MessageToDestination;
                MessageToOrigin = obj.MessageToOrigin;
                ToLocation = BackingDataCache.Get<IRoomData>(DataUtility.TryConvert<long>(obj.ToLocationID));
                VisibleStrength = obj.VisibleStrength;
                VisibleToSurroundings = obj.VisibleToSurroundings;

                DimensionalModelId = obj.Model.ModelBackingData.ID;
                DimensionalModelHeight = obj.Model.Height;
                DimensionalModelLength = obj.Model.Length;
                DimensionalModelWidth = obj.Model.Width;
                DimensionalModelVacuity = obj.Model.Vacuity;
                DimensionalModelCavitation = obj.Model.SurfaceCavitation;
                ModelDataObject = obj.Model;
            }
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
        
        [Display(Name = "Destination")]
        public long ToLocationID { get; set; }

        [Display(Name = "Origin")]
        public long FromLocationID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North")]
        public int DegreesFromNorth { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData ToLocation { get; set; }
        public IRoomData FromLocation { get; set; }
        public IPathwayData DataObject { get; set; }
    }
}