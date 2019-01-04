using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Zone;
using System;

namespace NetMud.Data.Gaias
{
    /// <summary>
    /// Controls weather over zones
    /// </summary>
    [Serializable]
    public class PressureSystem : IPressureSystem
    {
        /// <summary>
        /// The angle at which the pressure system runs along the planet
        /// STANDARD VALUE: 1-45
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        /// The direction the system moves
        /// </summary>
        public HemispherePlacement Direction { get; set; }

        /// <summary>
        /// How fast the system progresses forward
        /// STANDARD VALUE: 91014 - 136521
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// How strong the system currently is
        /// STANDARD VALUE: 1-100
        /// </summary>
        public float Strength { get; set; }

        /// <summary>
        /// How long is the system
        /// STANDARD VALUE: 31854 - 95562
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// What is the barometric pressure of the system
        /// STANDARD VALUE: Low 870-980; High 980-1050
        /// </summary>
        public float Pressure { get; set; }
    }
}
