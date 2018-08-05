using System;

namespace NutMud.Commands.Attributes
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandKeywordAttribute : Attribute
    {
        /// <summary>
        /// The keywords in question (exact word match, caps agnostic)
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// Is this keyword also the "subject" paramater for the command (see UseExits for the primary example)
        /// </summary>
        public bool IsAlsoSubject { get; private set; }
        
        /// <summary>
        /// When using Commands and Help should this keyword show up
        /// </summary>
        public bool DisplayInHelpAndCommands { get; private set; }

        /// <summary>
        /// Fairly obvious, stops this from being turned into a verb by the dictionary hoover
        /// </summary>
        public bool PreventBecomingAVerb { get; private set; }

        /// <summary>
        /// Creates a new keyword attribute
        /// </summary>
        /// <param name="keyword">The keywords in question (exact word match, caps agnostic)</param>
        /// <param name="isAlsoSubject">Is this keyword also the "subject" paramater for the command (see UseExits for the primary example)</param>
        /// <param name="displayInHelpAndCommands">When using Commands and Help should this keyword show up</param>
        public CommandKeywordAttribute(string keyword, bool isAlsoSubject, bool displayInHelpAndCommands = true, bool preventBecomingAVerb = false)
        {
            //Way easier just to load them all into lowercase so we don't have to move the cost to runtime
            if (string.IsNullOrWhiteSpace(keyword))
                throw (new ArgumentNullException(string.Format("{0} Command accessor keyword blank on implimentation.", GetType().ToString())));

            Keyword = keyword.ToLower();
            IsAlsoSubject = isAlsoSubject;
            DisplayInHelpAndCommands = displayInHelpAndCommands;
            PreventBecomingAVerb = preventBecomingAVerb;
        }
    }
}
