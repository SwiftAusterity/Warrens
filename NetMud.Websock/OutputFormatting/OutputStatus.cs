using System;
using System.Collections.Generic;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class OutputStatus
    {
        public string Occurrence { get; set; }
        public HashSet<Tuple<long, long, string>> VisibleMapDeltas { get; set; }
        public SelfStatus Self { get; set; }
        public LocalStatus Local { get; set; }
        public EnvironmentStatus Environment { get; set; }
        public string FullMap { get; set; }
        public string SoundToPlay { get; set; }
        public HashSet<InventoryItem> Inventory { get; set; }
    }

    [Serializable]
    public class InventoryItem
    {
        public string Name { get; set; }
        public int StackSize { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public string[] UseNames { get; set; }
    }
}
