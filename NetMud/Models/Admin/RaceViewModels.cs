using NetMud.Authentication;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class ManageRaceDataViewModel : PagedDataModel<IRace>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageRaceDataViewModel(IEnumerable<IRace> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IRace, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IRace, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IRace, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditRaceViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditRaceViewModel()
        {
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidItems = Enumerable.Empty<IInanimateTemplate>();
        }

        public IEnumerable<IZoneTemplate> ValidZones { get; set; }
        public IEnumerable<IInanimateTemplate> ValidItems { get; set; }
        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IRace DataObject { get; set; }
    }
}