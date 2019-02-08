using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageMineralsViewModel : PagedDataModel<IMineral>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMineralsViewModel(IEnumerable<IMineral> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IMineral, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IMineral, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IMineral, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditMineralsViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditMineralsViewModel()
        {
            ValidInanimateTemplates = Enumerable.Empty<IInanimateTemplate>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidMinerals = Enumerable.Empty<IMineral>();
        }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IEnumerable<IMineral> ValidMinerals { get; set; }
        public IMineral DataObject { get; set; }
    }
}