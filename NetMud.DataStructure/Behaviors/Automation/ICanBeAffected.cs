using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity can have affects removed or applied to them
    /// </summary>
    public interface ICanBeAffected : IHasAffects
    {
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
        AffectResistType DispelAffect(string affectTarget, Tuple<AffectType, int> dispellationMethod);
    }
}
