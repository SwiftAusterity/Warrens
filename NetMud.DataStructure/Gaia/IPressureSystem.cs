using NetMud.DataStructure.Zone;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Controls weather over zones
    /// </summary>
    public interface IPressureSystem
    {
        /// <summary>
        /// The angle at which the pressure system runs along the planet
        /// STANDARD VALUE: 1-45
        /// </summary>
        float Angle { get; set; }

        /// <summary>
        /// The direction the system moves
        /// </summary>
        HemispherePlacement Direction { get; set; }

        /// <summary>
        /// How fast the system progresses forward
        /// STANDARD VALUE: 91014 - 136521
        /// </summary>
        float Speed { get; set; }

        /// <summary>
        /// How strong the system currently is
        /// STANDARD VALUE: 1-100
        /// </summary>
        float Strength { get; set; }

        /// <summary>
        /// How long is the system
        /// STANDARD VALUE: 31854 - 95562
        /// </summary>
        float Size { get; set; }

        /// <summary>
        /// What is the barometric pressure of the system
        /// STANDARD VALUE: Low 870-980; High 980-1050
        /// </summary>
        float Pressure { get; set; }
    }
}
