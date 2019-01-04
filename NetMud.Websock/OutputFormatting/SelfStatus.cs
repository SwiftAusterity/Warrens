using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class SelfStatus
    {
        public int TotalHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int TotalStamina { get; set; }
        public int CurrentStamina { get; set; }
        public HashSet<IQuality> Qualities { get; set; }
    }
}
