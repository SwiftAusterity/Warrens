using NetMud.Authentication;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageFaunaViewModel : PagedDataModel<IFauna>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageFaunaViewModel(IEnumerable<IFauna> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IFauna, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IFauna, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IFauna, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditFaunaViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditFaunaViewModel()
        {
            ValidInanimateTemplates = Enumerable.Empty<IInanimateTemplate>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRaces = Enumerable.Empty<IRace>();
        }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IFauna DataObject { get; set; }
    }
}