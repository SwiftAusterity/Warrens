using System;

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
        /// The dispel resistance of the affect, -1 = undispellable
        /// </summary>
        int DispelResistance { get; set; }
    }
}
