using NetMud.DataStructure.Architectural.EntityBase;
using System;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Tracked statistics and quest progress
    /// </summary>
    [Serializable]
    public class Quality : IQuality
    {
        public string Name { get; set; }

        public int Value { get; set; }

        public bool Visible { get; set; }

        public QualityType Type { get; set; }
    }
}
