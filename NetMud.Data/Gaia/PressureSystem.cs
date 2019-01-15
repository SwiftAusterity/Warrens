using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Zone;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Gaia
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
        [Display(Name = "Angle", Description = "The angle at which the pressure system runs along the planet.")]
        [DataType(DataType.Text)]
        public float Angle { get; set; }

        /// <summary>
        /// The direction the system moves
        /// </summary>
        [Display(Name = "Direction", Description = "The direction the system moves.")]
        [DataType(DataType.Text)]
        public HemispherePlacement Direction { get; set; }

        /// <summary>
        /// How fast the system progresses forward
        /// STANDARD VALUE: 91014 - 136521
        /// </summary>
        [Display(Name = "Speed", Description = "How fast the system progresses forward.")]
        [DataType(DataType.Text)]
        public float Speed { get; set; }

        /// <summary>
        /// How strong the system currently is
        /// STANDARD VALUE: 1-100
        /// </summary>
        [Display(Name = "Strength", Description = "How strong the system currently is.")]
        [DataType(DataType.Text)]
        public float Strength { get; set; }

        /// <summary>
        /// How long is the system
        /// STANDARD VALUE: 31854 - 95562
        /// </summary>
        [Display(Name = "Size", Description = "How long is the system.")]
        [DataType(DataType.Text)]
        public float Size { get; set; }

        /// <summary>
        /// What is the barometric pressure of the system
        /// STANDARD VALUE: Low 870-980; High 980-1050
        /// </summary>
        [Display(Name = "Pressure", Description = "What is the barometric pressure of the system.")]
        [DataType(DataType.Text)]
        public float Pressure { get; set; }
    }
}
