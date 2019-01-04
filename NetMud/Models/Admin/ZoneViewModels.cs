using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class ManageZoneDataViewModel : PagedDataModel<IZoneTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageZoneDataViewModel(IEnumerable<IZoneTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IZoneTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditZoneDataViewModel : AddContentModel<IZoneTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("ZoneTemplateList")]
        [ZoneTemplateDataBinder]
        public override IZoneTemplate Template { get; set; }

        public AddEditZoneDataViewModel() : base(-1)
        {
        }

        public AddEditZoneDataViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {
                //todo
            }
        }

        public IEnumerable<IInanimateTemplate> ValidItems { get; set; }
        public IEnumerable<INonPlayerCharacterTemplate> ValidNPCs { get; set; }
        public IEnumerable<ITileTemplate> ValidTileTypes { get; set; }
        public IEnumerable<IGaiaTemplate> ValidWorlds { get; set; }
        public IZoneTemplate DataObject { get; set; }
    }

    public class AddZonePathwayDataViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddZonePathwayDataViewModel()
        {
            ValidZones = Enumerable.Empty<IZoneTemplate>();
        }

        [Display(Name = "Name", Description = "The identifying name and keyword of the pathway.")]
        [DataType(DataType.Text)]
        public string[] DestinationName { get; set; }

        [Display(Name = "Destination", Description = "The zone this leads to.")]
        [DataType(DataType.Text)]
        public long[] DestinationId { get; set; }

        [Display(Name = "Destination X", Description = "What coordinate tile this will send you to.")]
        [DataType(DataType.Text)]
        public short[] DestinationCoordinateX { get; set; }

        [Display(Name = "Destination Y", Description = "What coordinate tile this will send you to.")]
        [DataType(DataType.Text)]
        public short[] DestinationCoordinateY { get; set; }

        [Display(Name = "Origin X", Description = "What coordinate tile this originates from.")]
        [DataType(DataType.Text)]
        public short OriginCoordinateX { get; set; }

        [Display(Name = "Origin Y", Description = "What coordinate tile this originates from.")]
        [DataType(DataType.Text)]
        public short OriginCoordinateY { get; set; }

        [Display(Name = "Border Color", Description = "The hex code of the color of the border of the origin tile.")]
        [DataType(DataType.Text)]
        [UIHint("ColorPicker")]
        public string BorderHexColor { get; set; }

        public IEnumerable<IZoneTemplate> ValidZones { get; set; }

        public IZoneTemplate Origin { get; set; }
        public IPathway DataObject { get; set; }
    }
}