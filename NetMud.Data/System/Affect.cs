using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The dispel type of the affect
        /// </summary>
        public AffectType Type { get; set; }

        /// <summary>
        /// Chance of spread
        /// </summary>
        public Dictionary<ContagionVector, int> AfflictionChances { get; set; }

        public Affect()
        {
            Type = AffectType.Pure;
            Duration = -1;
            Value = 0;
            Target = String.Empty;
            AfflictionChances = new Dictionary<ContagionVector, int>();
        }

        public Affect(AffectType type, int duration, int value, string target, Dictionary<ContagionVector, int> afflictionChances)
        {
            Type = type;
            Duration = duration;
            Value = value;
            Target = target;
            AfflictionChances = afflictionChances;
        }

        /// <summary>
        /// Attempt to spread this to someone else
        /// </summary>
        /// <param name="affected">the afflcited</param>
        /// <param name="victim">the victim</param>
        /// <param name="vector">How this is being spread</param>
        /// <returns>success or failure</returns>
        public bool Afflict(IHasAffects source, ICanBeAffected victim, ContagionVector vector)
        {
            //TODO: math for strength of affliction based on stats/sktree
            return victim.ApplyAffect(this) == AffectResistType.Success;
        }


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
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.Target.Equals(this.Target, StringComparison.InvariantCultureIgnoreCase) && other.Type == this.Type)
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
                    return other.GetType() == this.GetType() 
                        && other.Target.Equals(this.Target, StringComparison.InvariantCultureIgnoreCase) 
                        && other.Type == this.Type;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
    }
}
