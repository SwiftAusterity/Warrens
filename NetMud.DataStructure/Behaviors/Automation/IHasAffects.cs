using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity can have affects attached to them
    /// </summary>
    public interface IHasAffects
    {
        /// <summary>
        /// The affects.. affecting the entity
        /// </summary>
        HashSet<IAffect> Affects { get; }

        /// <summary>
        /// Checks if there is an affect without having to crawl the hashset everytime
        /// </summary>
        /// <param name="affectTarget">the target of the affect</param>
        /// <returns>if it exists or not</returns>
        bool HasAffect(string affectTarget);

        /// <summary>
        /// Attempts to apply the affect
        /// </summary>
        /// <param name="affectToApply">the affect to apply</param>
        /// <returns>what type of resist happened (or success)</returns>
        AffectResistType ApplyAffect(IAffect affectToApply);

        /// <summary>
        /// Attempt to dispel the affect
        /// </summary>
        /// <param name="affectTarget">the thing attempting to be dispeled</param>
        /// <returns>reisst type</returns>
        AffectResistType DispelAffect(string affectTarget);
    }

    public enum AffectResistType
    {
        Success,
        Resisted,
        Immune
    }
}
