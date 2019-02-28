using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Gaia
{
    [Serializable]
    public class MeterologicalFront
    {
        /// <summary>
        /// The system
        /// </summary>
        [UIHint("PressureSystem")]
        public IPressureSystem Event { get; set; }

        /// <summary>
        /// Where the front is on the planet in its movement cycle.
        /// </summary>
        [Display(Name = "Global Position", Description = "Where the front is on the planet in its movement cycle.")]
        [DataType(DataType.Text)]
        public float Position { get; set; }

        public MeterologicalFront()
        {
            Position = 0;
        }

        public MeterologicalFront(IPressureSystem weatherEvent, float position)
        {
            Event = weatherEvent;
            Position = position;
        }
    }
}
