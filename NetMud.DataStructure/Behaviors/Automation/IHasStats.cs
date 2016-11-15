using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity has RPG style stats (strength, etc)
    /// </summary>
    public interface IHasStats
    {
        Dictionary<IStat, int> Stats { get; }
    }
}
