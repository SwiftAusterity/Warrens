using NetMud.DataStructure.Behaviors.Actionable;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Physical ailments
    /// </summary>
    public interface IAilment : IComparable<IAilment>, IEquatable<IAilment>
    {
        /// <summary>
        /// The value that the target is affected by
        /// </summary>
        int Severity { get; set; }

        /// <summary>
        /// How the severity is affected per cycle
        /// </summary>
        int Step { get; set; }

        /// <summary>
        /// The type of the ailment
        /// </summary>
        AilmentType Type { get; set; }

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
        //bool Afflict(ICanBeWounded affected, ICanBeWounded victim, ContagionVector vector);
    }

    /// <summary>
    /// Type of affect for dispelling purposes
    /// </summary>
    public enum AilmentType
    {
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
        Poison
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
