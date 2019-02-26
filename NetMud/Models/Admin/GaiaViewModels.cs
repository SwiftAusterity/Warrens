using NetMud.Authentication;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.Data.Gaia;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class ManageGaiaViewModel : PagedDataModel<IGaiaTemplate>
    {
        public ManageGaiaViewModel(IEnumerable<IGaiaTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGaiaTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IGaiaTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IGaiaTemplate, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditGaiaViewModel : AddEditTemplateModel<IGaiaTemplate>
    {
        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("NonPlayerCharacterTemplateList")]
        [GaiaTemplateDataBinder]
        public override IGaiaTemplate Template { get; set; }

        public AddEditGaiaViewModel() : base(-1)
        {
            ValidCelestials = TemplateCache.GetAll<ICelestial>(true);
            DataObject = new GaiaTemplate();
        }

        public AddEditGaiaViewModel(long templateId) : base(templateId)
        {
            ValidCelestials = TemplateCache.GetAll<ICelestial>(true);
            DataObject = new GaiaTemplate();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.CelestialBodies = DataTemplate.CelestialBodies;
                DataObject.ChronologicalSystem = DataTemplate.ChronologicalSystem;
                DataObject.Qualities = DataTemplate.Qualities;
                DataObject.RotationalAngle = DataTemplate.RotationalAngle;
            }
        }

        public AddEditGaiaViewModel(string archivePath, IGaiaTemplate item) : base(archivePath, item)
        {
            ValidCelestials = TemplateCache.GetAll<ICelestial>(true);
            DataObject = item;
        }

        public IEnumerable<ICelestial> ValidCelestials { get; set; }
        public IGaiaTemplate DataObject { get; set; }
    }
}