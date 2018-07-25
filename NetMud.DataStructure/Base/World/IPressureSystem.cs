using NetMud.DataStructure.Base.Place;

namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Controls weather over zones
    /// </summary>
    public interface IPressureSystem
    {
        /// <summary>
        /// The angle at which the pressure system runs along the planet
        /// </summary>
        float Angle { get; set; }

        /// <summary>
        /// The direction the system moves
        /// </summary>
        HemispherePlacement Direction { get; set; }

        /// <summary>
        /// How fast the system progresses forward
        /// </summary>
        float Speed { get; set; }

        /// <summary>
        /// How strong the system currently is
        /// </summary>
        float Strength { get; set; }

        /// <summary>
        /// How long is the system
        /// </summary>
        float Size { get; set; }

        /// <summary>
        /// What is the barometric pressure of the system
        /// </summary>
        float Pressure { get; set; }
    }
}
