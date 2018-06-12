using NetMud.Data.Serialization;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
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

        public Occurrence()
        {

        }

        public Occurrence(ILexica happening, int strength)
        {
            Event = happening;
            Strength = strength;
        }
    }
}
