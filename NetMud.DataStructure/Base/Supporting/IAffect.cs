using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Enchantment affect applied
    /// </summary>
    public interface IAffect : IComparable<IAffect>, IEquatable<IAffect>
    {
        /// <summary>
        /// The target, is free text
        /// </summary>
        string Target { get; set; }

        /// <summary>
        /// The value that the target is affected by
        /// </summary>
        int Value { get; set; }

        /// <summary>
        /// The time duration of the affect, base duration on backingdata
        /// </summary>
        int Duration { get; set; }

        /// <summary>
        /// The dispel type of the affect
        /// </summary>
        AffectType Type { get; set; }

        /// <summary>
        /// Chance of spread
        /// </summary>
        Dictionary<ContagionVector, int> AfflictionChances { get; set; }

        /// <summary>
        /// Attempt to spread this to someone else
        /// </summary>
        /// <param name="affected">the afflcited</param>
        /// <param name="victim">the victim</param>
        /// <param name="vector">How this is being spread</param>
        /// <returns>success or failure</returns>
        bool Afflict(IHasAffects source, ICanBeAffected victim, ContagionVector vector);
    }

    /// <summary>
    /// Type of affect for dispelling purposes
    /// </summary>
    public enum AffectType
    {
        /// <summary>
        /// Can be dispelling magically
        /// </summary>
        Magical,

        /// <summary>
        /// Undispelable by magical means, medically treatable
        /// </summary>
        Physical,

        /// <summary>
        /// Is a pathogenic condition
        /// </summary>
        Disease,

        /// <summary>
        /// Is a venom
        /// </summary>
        Poison,

        /// <summary>
        /// Undispellable by any means period
        /// </summary>
        Pure
    }

    /// <summary>
    /// Vectors by which this contagion can be spread
    /// </summary>
    public enum ContagionVector
    {
        Pneumatic,
        DirectContact,
        IntimateContact
    }
}
