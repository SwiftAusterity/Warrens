using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Data.Reference;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Web;

namespace NetMud.Models.GameAdmin
{
    public class DashboardViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateData>();
            Rooms = Enumerable.Empty<IRoomData>();
            NPCs = Enumerable.Empty<INonPlayerCharacter>();

            DimensionalModels = Enumerable.Empty<IDimensionalModelData>();
            HelpFiles = Enumerable.Empty<Help>();
            Materials = Enumerable.Empty<IMaterial>();

            LiveRooms = 0;
            LiveInanimates = 0;
            LiveNPCs = 0;

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<IRoomData> Rooms { get; set; }
        public IEnumerable<IInanimateData> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }

        //Reference Data
        public IEnumerable<IDimensionalModelData> DimensionalModels { get; set; }
        public IEnumerable<Help> HelpFiles { get; set; }
        public IEnumerable<IMaterial> Materials { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public int LiveRooms { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }
    }

    #region Help
    public class ManageHelpDataViewModel : PagedDataModel<Help>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageHelpDataViewModel(IEnumerable<Help> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<Help, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.HelpText.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditHelpDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditHelpDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [DataType(DataType.Text)]
        [Display(Name = "HelpText")]
        public string NewHelpText { get; set; }


        public Help DataObject { get; set; }
    }
    #endregion

    #region Materials
    public class ManageMaterialDataViewModel : PagedDataModel<Material>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMaterialDataViewModel(IEnumerable<Material> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<Material, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditMaterialViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditMaterialViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Conductive")]
        public bool NewConductive { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Magnetic")]
        public bool NewMagnetic { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Is Flammable")]
        public bool NewFlammable { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Viscosity")]
        public short NewViscosity{ get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Density")]
        public short NewDensity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Mallebility")]
        public short NewMallebility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Ductility")]
        public short NewDuctility { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Porosity")]
        public short NewPorosity { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Fusion Point")]
        public short NewSolidPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Vaporization Point")]
        public short NewGasPoint { get; set; }

        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Temperature Retention")]
        public short NewTemperatureRetention { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Damage Resistance")]
        public short[] Resistances { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Damage Resistance Value")]
        public short[] ResistanceValues { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Material Composition")]
        public long[] Compositions { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Material Composition Percentage")]
        public short[] CompositionPercentages { get; set; }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public Material DataObject { get; set; }
    }
    #endregion

    #region Models
    public class ManageDimensionalModelDataViewModel : PagedDataModel<DimensionalModelData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageDimensionalModelDataViewModel(IEnumerable<DimensionalModelData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<DimensionalModelData, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditDimensionalModelDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDimensionalModelDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Model Planes Upload")]
        public HttpPostedFileBase ModelFile { get; set; }

        public DimensionalModelData DataObject { get; set; }
    }
    #endregion

    #region Inanimates
    public class ManageInanimateDataViewModel : PagedDataModel<IInanimateData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateDataViewModel(IEnumerable<IInanimateData> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IInanimateData, bool> SearchFilter 
        { 
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditInanimateDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditInanimateDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Containers")]
        public string[] InanimateContainerNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Container Weights")]
        public long[] InanimateContainerWeights { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Inanimate Container Volumes")]
        public long[] InanimateContainerVolumes { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Containers")]
        public string[] MobileContainerNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Container Weights")]
        public long[] MobileContainerWeights { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Character Container Volumes")]
        public long[] MobileContainerVolumes { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Length (inches)")]
        public int DimensionalModelLength { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Height (inches)")]
        public int DimensionalModelHeight { get; set; }

        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Width (inches)")]
        public int DimensionalModelWidth { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Dimensional Model")]
        public long DimensionalModelId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Model Parts")]
        public string[] ModelPartNames { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Model Part Materials")]
        public long[] ModelPartMaterials { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Internal Composition")]
        public long[] InternalCompositionIds { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Composition Percentage")]
        public short[] InternalCompositionPercentages { get; set; }

        public IEnumerable<IInanimateData> ValidInanimateDatas { get; set; }
        public IEnumerable<IDimensionalModelData> ValidModels { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }

        public IInanimateData DataObject { get; set; }
    }
    #endregion

    #region Rooms
    public class ManageRoomDataViewModel : PagedDataModel<IRoomData>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRoomDataViewModel(IEnumerable<IRoomData> items) : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRoomData, bool> SearchFilter 
        { 
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditRoomDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditRoomDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string NewName { get; set; }

        public IRoomData DataObject { get; set; }
    }
    #endregion

    #region NPC
    public class ManageNPCDataViewModel : PagedDataModel<INonPlayerCharacter>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageNPCDataViewModel(IEnumerable<INonPlayerCharacter> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<INonPlayerCharacter, bool> SearchFilter 
        { 
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.SurName.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditNPCDataViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditNPCDataViewModel()
        {
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Given Name")]
        public string NewName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Family Name")]
        public string NewSurName { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string NewGender { get; set; }

        public INonPlayerCharacter DataObject { get; set; }
    }
    #endregion

    #region Players
    public class ManagePlayersViewModel : PagedDataModel<ApplicationUser>, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManagePlayersViewModel(IEnumerable<ApplicationUser> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
            ValidRoles = Enumerable.Empty<IdentityRole>();
        }

        internal override Func<ApplicationUser, bool> SearchFilter 
        { 
            get
            {
                return item => item.GameAccount.GlobalIdentityHandle.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        public IEnumerable<IdentityRole> ValidRoles { get; set; }
    }
#endregion
}