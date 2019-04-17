using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Combat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageCombosViewModel : PagedDataModel<IFightingArtCombination>
    {
        public ManageCombosViewModel(IEnumerable<IFightingArtCombination> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFightingArtCombination, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFightingArtCombination, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFightingArtCombination, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditCombosViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public AddEditCombosViewModel()
        {
            ValidArts = TemplateCache.GetAll<IFightingArt>();
        }

        public IEnumerable<IFightingArt> ValidArts { get; set; }

        [UIHint("FightingArtCombination")]
        public IFightingArtCombination DataObject { get; set; }
    }
}