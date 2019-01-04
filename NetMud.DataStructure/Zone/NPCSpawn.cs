using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Zone
{
    public class NPCSpawn
    {
        /// <summary>
        /// Where this should spawn
        /// </summary>
        public Coordinate Placement { get; set; }

        /// <summary>
        /// What should spawn
        /// </summary>
        public long NPCId { get; set; }
    }
}
