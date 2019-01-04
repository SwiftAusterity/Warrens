using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageTileDataViewModel : PagedDataModel<ITileTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageTileDataViewModel(IEnumerable<ITileTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<ITileTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditTileDataViewModel : AddContentModel<ITileTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("TileTemplateList")]
        [TileTemplateDataBinder]
        public override ITileTemplate Template { get; set; }

        public AddEditTileDataViewModel() : base(-1)
        {
        }

        public AddEditTileDataViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {
            }
        }

        [UIHint("TileTemplate")]
        public ITileTemplate DataObject { get; set; }
    }
}