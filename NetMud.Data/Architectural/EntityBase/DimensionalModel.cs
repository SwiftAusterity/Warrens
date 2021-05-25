using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// The live version of a dimensional model
    /// </summary>
    [Serializable]
    public class DimensionalModel : IDimensionalModel
    {
        /// <summary>
        /// Y axis of the 21 plane model
        /// </summary>
        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Length (inches)", Description = "The dimensional length of this model.")]
        [DataType(DataType.Text)]
        public int Length { get; set; }

        /// <summary>
        /// Measurement of all 21 planes vertically
        /// </summary>
        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Height (in)", Description = "The dimensional height of this model.")]
        [DataType(DataType.Text)]
        public int Height { get; set; }

        /// <summary>
        /// X axis of the 21 plane model
        /// </summary>
        [Range(1, 1200, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Width (in)", Description = "The dimensional width of this model.")]
        [DataType(DataType.Text)]
        public int Width { get; set; }

        /// <summary>
        /// How hollow something is, we have to maintain current vacuity versus the spawned vacuity in the ModelData
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness", Description = "The hollowness of the model. Hollowness can increase sturctural integrity up to a point.")]
        [DataType(DataType.Text)]
        public int Vacuity { get; set; }

        /// <summary>
        /// How pock-marked the surface areas are of the object
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Surface Cavitation", Description = "The cavitation of the model surface. Cavitation can decrease sturctural integrity and account for things passing through.")]
        [DataType(DataType.Text)]
        public int SurfaceCavitation { get; set; }

        [JsonProperty("ModelTemplate")]
        private TemplateCacheKey _modelTemplate { get; set; }

        /// <summary>
        /// The model we're following
        /// </summary>

        [JsonIgnore]
        [Display(Name = "Model", Description = "The model we're following.")]
        [UIHint("DimensionalModelDataList")]
        [DimensionalModelDataBinder]
        public IDimensionalModelData ModelTemplate
        {
            get
            {
                if (_modelTemplate != null)
                {
                    return TemplateCache.Get<IDimensionalModelData>(_modelTemplate);
                }
                else
                {
                    // 0d models don't have real values
                    DimensionalModelData returnValue = new()
                    {
                        ModelType = DimensionalModelType.None
                    };

                    return returnValue;
                }
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _modelTemplate = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [UIHint("ModelPartCompositions")]
        public HashSet<IModelPartComposition> Composition { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DimensionalModel()
        {
            Composition = new HashSet<IModelPartComposition>();

            ModelTemplate = new DimensionalModelData
            {
                ModelType = DimensionalModelType.None
            };
        }

        /// <summary>
        /// Constructor for dimensional model based on full specific data
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        /// <param name="backingDataKey">dimensional model backing data id</param>
        /// <param name="materialComps">The material compositions</param>
        public DimensionalModel(int length, int height, int width, int vacuity, int surfaceCavitation, TemplateCacheKey backingDataKey, HashSet<IModelPartComposition> materialComps)
        {
            Length = length;
            Height = height;
            Width = width;
            Vacuity = vacuity;
            SurfaceCavitation = surfaceCavitation;
            Composition = materialComps;

            _modelTemplate = backingDataKey;
        }

        /// <summary>
        /// constructor for 0 dimension models
        /// </summary>
        /// <param name="length">Length parameter of the model</param>
        /// <param name="height">Height parameter of the model</param>
        /// <param name="width">Width parameter of the model</param>
        public DimensionalModel(int length, int height, int width, int vacuity, int surfaceCavitation)
        {
            Length = length;
            Height = height;
            Width = width;
            Vacuity = vacuity;
            SurfaceCavitation = surfaceCavitation;
            Composition = new HashSet<IModelPartComposition>();

            ModelTemplate = new DimensionalModelData
            {
                ModelType = DimensionalModelType.None
            };
        }
    }
}
