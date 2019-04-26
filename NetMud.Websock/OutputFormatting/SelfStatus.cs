using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class SelfStatus
    {
        public BodyStatus Body { get; set; }
        public string Position { get; set; }
        public string Stance { get; set; }
        public string Balance { get; set; }
        public string Stagger { get; set; }
        public string CurrentActivity { get; set; }
        public string CurrentArt { get; set; }
        public string CurrentCombo { get; set; }
        public string CurrentTarget { get; set; }
        public double CurrentTargetHealth { get; set; }
    }
}
