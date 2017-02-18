using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity or data point can have affects attached to them
    /// </summary>
    public interface IHasAffects
    {
        /// <summary>
        /// The affects.. affecting the entity
        /// </summary>
        HashSet<IAffect> Affects { get; }

        /// <summary>
        /// Checks if there is an affect without having to crawl the hashset everytime or returning a big class object
        /// </summary>
        /// <param name="affectTarget">the target of the affect</param>
        /// <returns>if it exists or not</returns>
        bool HasAffect(string affectTarget);
    }

    public enum AffectResistType
    {
        Success,
        Resisted,
        Immune
    }
}
