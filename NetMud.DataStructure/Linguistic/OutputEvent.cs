using System.Collections.Generic;
using System.Text;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Collective of sensory events to produce output from
    /// </summary>
    public class OutputEvent
    {
        /// <summary>
        /// Collection of sensory events
        /// </summary>
        public List<ISensoryEvent> Events { get; set; }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void AddEvent(ISensoryEvent modifier)
        {
            if (modifier == null)
            {
                return;
            }

            Events.Add(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void AddEvent(ISensoryEvent[] modifier)
        {
            if (modifier == null)
            {
                return;
            }

            Events.AddRange(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void AddEvent(IEnumerable<ISensoryEvent> modifier)
        {
            if (modifier == null)
            {
                return;
            }

            Events.AddRange(modifier);
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public string Unpack(LexicalContext overridingContext = null, bool omitName = true)
        {
            var sb = new StringBuilder();

            //TODO: Maybe add a toggle to separate/style output by sense
            foreach(var sense in Events)
            {
                sb.Append(sense.Unpack(overridingContext, omitName));
            }

            return sb.ToString().Trim();
        }
    }
}
