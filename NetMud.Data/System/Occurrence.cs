using NetMud.Communication.Messaging;
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

        /// <summary>
        /// Use this to create a "blank" occurrence
        /// </summary>
        public Occurrence(MessagingType sensoryType)
        {
            SensoryType = sensoryType;
            Strength = -1;
            Event = new Lexica(LexicalType.Noun, GrammaticalType.Descriptive, string.Empty);
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <returns>a sentence fragment</returns>
        public override string ToString()
        {
            return Event.ToString();
        }
    }
}
