using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;

namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Celestial bodies
    /// </summary>
    public interface ICelestial : ILookupData, IDescribable, ILookable, ISensible, IRenderInLocation
    {
        /// <summary>
        /// Orbit Type
        /// </summary>
        CelestialOrientation OrientationType { get; set; }

        /// <summary>
        /// Zenith distance of an elliptical orbit
        /// </summary>
        int Apogee { get; set; }

        /// <summary>
        /// Minimal distance of an elliptical orbit
        /// </summary>
        int Perigree { get; set; }

        /// <summary>
        /// How fast is this going through space
        /// </summary>
        int Velocity { get; set; }

        /// <summary>
        /// How bright is this thing
        /// </summary>
        int Luminosity { get; set; }

        /// <summary>
        /// Physical model for the celestial object
        /// </summary>
        IDimensionalModel Model { get; set; }
    }
}
