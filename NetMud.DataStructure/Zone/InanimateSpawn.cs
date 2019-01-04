using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Zone
{
    public class InanimateSpawn
    {
        /// <summary>
        /// Where this should spawn
        /// </summary>
        public Coordinate Placement { get; set; }

        /// <summary>
        /// What to spawn
        /// </summary>
        public long ItemId { get; set; }
    }
}