using NetMud.Authentication;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageMaterialDataViewModel : PagedDataModel<IMaterial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageMaterialDataViewModel(IEnumerable<IMaterial> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IMaterial, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IMaterial, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IMaterial, object> OrderSecondary
        {
            get
            {
                return null;
            }
        }
    }

    public class AddEditMaterialViewModel : AddEditTemplateModel<IMaterial>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Apply Existing Template", Description = "Apply an existing object's data to this new data.")]
        [UIHint("MaterialList")]
        [MaterialDataBinder]
        public override IMaterial Template { get; set; }

        public AddEditMaterialViewModel() : base(-1)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            DataObject = new Material();
        }

        public AddEditMaterialViewModel(long templateId) : base(templateId)
        {
            ValidMaterials = TemplateCache.GetAll<IMaterial>();
            DataObject = new Material();

            //apply template
            if (DataTemplate != null)
            {
                DataObject.AccumulationCap = DataTemplate.AccumulationCap;
                DataObject.Composition = DataTemplate.Composition;
                DataObject.Conductive = DataTemplate.Conductive;
                DataObject.Density = DataTemplate.Density;
                DataObject.Ductility = DataTemplate.Ductility;
                DataObject.Flammable = DataTemplate.Flammable;
                DataObject.GasPoint = DataTemplate.GasPoint;
                DataObject.Magnetic = DataTemplate.Magnetic;
                DataObject.Mallebility = DataTemplate.Mallebility;
                DataObject.Porosity = DataTemplate.Porosity;
                DataObject.Resistance = DataTemplate.Resistance;
                DataObject.SolidPoint = DataTemplate.SolidPoint;
                DataObject.TemperatureRetention = DataTemplate.TemperatureRetention;
                DataObject.Viscosity = DataTemplate.Viscosity;
            }
        }

        public IEnumerable<IMaterial> ValidMaterials { get; set; }
        public IMaterial DataObject { get; set; }
    }
}