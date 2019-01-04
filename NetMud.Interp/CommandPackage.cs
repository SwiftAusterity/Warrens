using System;
using System.Collections.Generic;

namespace NetMud.Interp
{
    /// <summary>
    /// Initial parsing package for finding and building commands
    /// </summary>
    public class CommandPackage
    {
        /// <summary>
        /// The command's type
        /// </summary>
        public Type CommandType { get; set; }

        /// <summary>
        /// The keyword used to find the command
        /// </summary>
        public string CommandPhrase { get; set; }

        /// <summary>
        /// The remainder words of the input string
        /// </summary>
        public IEnumerable<string> InputRemainder { get; set; }
    }
}
