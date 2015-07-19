using NetMud.DataStructure.Behaviors.Rendering;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    public interface ICommand
    {
        IEnumerable<string> Execute();

        IEnumerable<string> RenderSyntaxHelp();

        /* 
         * Syntax:
         *      command <subject> <target> <supporting>
         *  Location is derived from context
         *  Surroundings is derived from location
         */

        IActor Actor { get; set; }
        object Subject { get; set; }
        object Target { get; set; }
        object Supporting { get; set; }
        ILocation OriginLocation { get; set; }
        IEnumerable<ILocation> Surroundings { get; set; }
    }
}
