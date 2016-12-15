using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Actionable
{
    public interface ICanBeWounded
    {
        /// <summary>
        /// List of ailments: ailment, bodypart
        /// </summary>
        IEnumerable<Tuple<IAilment, string>> Ailments { get; }

        /// <summary>
        /// Inflict a wound here
        /// </summary>
        /// <param name="source">the origin, nullable</param>
        /// <param name="victim">the victim</param>
        /// <returns>success or failure</returns>
        bool Inflict(IEntity source);

        /// <summary>
        /// Attempt to cure this of something
        /// </summary>
        /// <param name="source">the source trying to fix the problem, nullable</param>
        /// <param name="strength">How strong the attempt is</param>
        /// <returns>Was something cured</returns>
        string Cure(IEntity source, int strength)
    }
}
