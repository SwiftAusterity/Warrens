using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Defines methods and properties for containing liquid materials
    /// </summary>
    public interface IContainsLiquid
    {
        /// <summary>
        /// What liquids are in this
        /// </summary>
        Dictionary<IMaterial, int> LiquidsContained { get; set; }

        /// <summary>
        /// Put liquid in this container
        /// </summary>
        /// <param name="source">Where the liquid is coming from</param>
        /// <param name="liquid">The type of liquid to fill this with</param>
        /// <param name="amount">How much we're filling. defaults to -1 which means as much as we can</param>
        /// <returns>error or success message</returns>
        string FillWithLiquid(IContainsLiquid source, IMaterial liquid, int amount = -1);

        /// <summary>
        /// Drains liquid. You can't choose which liquid it drains whatever it wants
        /// </summary>
        /// <param name="destination">Where the liquid is going</param>
        /// <param name="amount">how much to drain</param>
        /// <returns>success or error message</returns>
        string DrainLiquid(IContainsLiquid destination, int amount = -1);
    }
}
