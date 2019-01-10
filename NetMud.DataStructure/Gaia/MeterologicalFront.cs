using System;

namespace NetMud.DataStructure.Gaia
{
    [Serializable]
    public class MeterologicalFront
    {
        public IPressureSystem Event { get; set; }
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
