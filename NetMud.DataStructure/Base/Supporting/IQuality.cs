using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Tracked statistics and quest progress
    /// </summary>
    public interface IQuality
    {
        string Name { get; set; }

        int Value { get; set; }

        bool Visible { get; set; }

        string Description { get; set; }

        QualityType Type { get; set; }
    }

    public enum QualityType
    {
        Quest,
        Kill,
        Statistic
    }
}
