using NetMud.Authentication;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
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

    public class AddEditNPCDataViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditNPCDataViewModel()
        {
        }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateTemplate> ValidItems { get; set; }

        [UIHint("NonPlayerCharacterTemplate")]
        public INonPlayerCharacterTemplate DataObject { get; set; }
    }
}