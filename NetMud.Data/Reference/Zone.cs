using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : ReferenceDataPartial, IZone
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        public int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        public int PressureCoefficient { get; set; }

        /// <summary>
        /// Who currently owns this zone
        /// </summary>
        public long Owner { get; set; }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";

            BaseElevation = 0;
            TemperatureCoefficient = 0;
            PressureCoefficient = 0;
            Owner = -1;
            Claimable = false;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            return base.RenderHelpBody();
        }
    }
}
