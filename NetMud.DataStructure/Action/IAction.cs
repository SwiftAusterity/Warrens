using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Tile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Action
{

    /// <summary>
    /// The basis for action classes
    /// </summary>
    public interface IAction : ICloneable
    {
        /// <summary>
        /// Name and keyword for the interaction
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// How much stamina does this cost to use
        /// </summary>
        int StaminaCost { get; set; }

        /// <summary>
        /// How much health does this cost to use
        /// </summary>
        int HealthCost { get; set; }

        /// <summary>
        /// Does this interaction have an embedded sound file that plays when it's done
        /// </summary>
        string FoleyUri { get; set; }

        /// <summary>
        /// Message to send to the origin location of the command/event
        /// </summary>
        string ToLocalMessage { get; set; }

        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        string ToActorMessage { get; set; }

        /// <summary>
        /// The pattern this affects, can hit multiple things potentially. (viewed as a collection of (X,Y) deltas)
        /// </summary>
        HashSet<Coordinate> AffectPattern { get; set; }

        /// <summary>
        /// List of criteria to use this
        /// </summary>
        HashSet<IActionCriteria> Criteria { get; set; }

        /// <summary>
        /// List of results of using this 
        /// </summary>
        HashSet<IActionResult> Results { get; set; }

        /// <summary>
        /// Invokes this action
        /// </summary>
        /// <param name="actor">The one doing the action</param>
        /// <param name="initialTarget">The initial target</param>
        /// <param name="tool">The tool or use-item</param>
        /// <returns>An error message (or blank for success)</returns>
        string Invoke(IEntity actor, ITile initialTarget, IInanimate tool);
    }
}
