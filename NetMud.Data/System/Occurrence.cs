using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.Data.System
{
    [Serializable]
    public class Occurrence : IOccurrence
    {
        /// <summary>
        /// The thing happening
        /// </summary>
        public ILexica Event { get; set; }

        /// <summary>
        /// The perceptive strength (higher = easier to see and greater distance noticed)
        /// </summary>
        public int Strength { get; set; }

        /// <summary>
        /// The type of sense used to detect this
        /// </summary>
        public MessagingType SensoryType { get; set; }

        public Occurrence()
        {

        }

        public Occurrence(ILexica happening, int strength, MessagingType sensoryType)
        {
            Event = happening;
            Strength = strength;
            SensoryType = sensoryType;
        }
    }
}
