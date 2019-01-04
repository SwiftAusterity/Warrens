using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.NPCs;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageNPCDataViewModel : PagedDataModel<INonPlayerCharacterTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageNPCDataViewModel(IEnumerable<INonPlayerCharacterTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<INonPlayerCharacterTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.SurName.ToLower().Contains(SearchTerms.ToLower());
            }
        }

    }

    public class AddEditNPCDataViewModel : AddContentModel<INonPlayerCharacterTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [NonPlayerCharacterTemplateDataBinder]
        public override INonPlayerCharacterTemplate Template { get; set; }

        public AddEditNPCDataViewModel() : base(-1)
        {
        }

        public AddEditNPCDataViewModel(long templateId) : base(templateId)
        {
            //apply template
            if (DataTemplate == null)
            {
                //set defaults
            }
            else
            {
                //TODO
            }
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateDatas { get; set; }
        public IEnumerable<INonPlayerCharacterTemplate> ValidNPCDatas { get; set; }
        public IEnumerable<ITileTemplate> ValidTileDatas { get; set; }

        [UIHint("NonPlayerCharacterTemplate")]
        public NonPlayerCharacterTemplate DataObject { get; set; }
    }
}