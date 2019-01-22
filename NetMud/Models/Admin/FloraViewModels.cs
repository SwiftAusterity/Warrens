using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFloraViewModel : PagedDataModel<IFlora>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFloraViewModel(IEnumerable<IFlora> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFlora, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditFloraViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditFloraViewModel()
        {
            ValidInanimateTemplates = Enumerable.Empty<IInanimateTemplate>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFlora DataObject { get; set; }
    }
}