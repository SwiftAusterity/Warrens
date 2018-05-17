using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using System;

namespace NetMud.Data.System
{
    /// <summary>
    /// Enchantment affect applied
    /// </summary>
    [Serializable]
    public class Affect : IAffect
    {
        /// <summary>
        /// The target, is free text
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The value that the target is affected by
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The time duration of the affect, base duration on backingdata
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Chance of spread
        /// </summary>
        public int DispelResistance { get; set; }

        /// <summary>
        /// Make a new affect with default values
        /// </summary>
        public Affect()
        {
            Duration = -1;
            Value = 0;
            Target = String.Empty;
            DispelResistance = 0;
        }

        /// <summary>
        /// Make a new affect with values
        /// </summary>
        /// <param name="duration">How long this affect runs</param>
        /// <param name="value">The strength of the affect</param>
        /// <param name="target">The target attribute this affects</param>
        /// <param name="dispelResistance">How hard is it to remove and the transmission chance</param>
        public Affect(int duration, int value, string target, int dispelResistance)
        {
            Duration = duration;
            Value = value;
            Target = target;
            DispelResistance = dispelResistance;
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = not the same
        /// 1 = same reference (same name, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IAffect other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Target.Equals(Target, StringComparison.InvariantCultureIgnoreCase))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IAffect other)
        {
            if (other != default(IAffect))
            {
                try
                {
                    return other.GetType() == GetType() 
                        && other.Target.Equals(Target, StringComparison.InvariantCultureIgnoreCase);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
    }
}
