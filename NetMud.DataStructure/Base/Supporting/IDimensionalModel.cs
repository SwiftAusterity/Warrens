using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Framework for the physics model of an entity. This is an in-transit object.
    /// </summary>
    public interface IDimensionalModel
    {
        /// <summary>
        /// Y axis of the 21 plane model
        /// </summary>
        
        int Length { get; set; }

        /// <summary>
        /// Measurement of all 21 planes vertically
        /// </summary>
        
        int Height { get; set; }

        /// <summary>
        /// X axis of the 21 plane model
        /// </summary>
        
        int Width { get; set; }

        /// <summary>
        /// How hollow something is, we have to maintain current vacuity versus the spawned vacuity in the ModelData
        /// </summary>
        
        int Vacuity { get; set; }

        /// <summary>
        /// How pock-marked the surface areas are of the object
        /// </summary>
        
        int SurfaceCavitation { get; set; }

        /// <summary>
        /// The model we're following
        /// </summary>
        IDimensionalModelData ModelBackingData { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        IDictionary<string, IMaterial> Composition { get; set; }
    }
}
