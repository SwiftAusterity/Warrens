using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageInanimateTemplateViewModel : PagedDataModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateTemplateViewModel(IEnumerable<IInanimateTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IInanimateTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IInanimateTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IInanimateTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditInanimateTemplateViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditInanimateTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }

        [UIHint("InanimateTemplate")]
        public IInanimateTemplate DataObject { get; set; }
    }
}