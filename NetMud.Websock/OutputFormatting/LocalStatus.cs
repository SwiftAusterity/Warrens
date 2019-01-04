﻿using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class LocalStatus
    {
        public string ZoneName { get; set; }
        public string Tile { get; set; }
        public string LocationDescriptive { get; set; }
        public string[] Exits { get; set; }
        public string[] Populace { get; set; }
        public string[] Inventory { get; set; }
    }
}
