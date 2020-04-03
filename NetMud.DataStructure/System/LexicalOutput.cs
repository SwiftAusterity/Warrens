using System;

namespace NetMud.DataStructure.System
{
    [Serializable]
    public class LexicalOutput
    {
        public string Description { get; set; }
        public bool Success { get; set; }
        public string[] Errors { get; set; }
    }
}
