using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// Idenfies methods and a component list for crafting
    /// </summary>
    public interface ICanBeCrafted
    {
        /// <summary>
        /// The amount produced
        /// </summary>
        int Produces { get; set; }

        /// <summary>
        /// Object type + amount needed to craft this
        /// </summary>
        IEnumerable<IInanimateComponent> Components { get; set; }

        /// <summary>
        /// List of quality/value pairs needed by the crafter
        /// </summary>
        HashSet<QualityValue> SkillRequirements { get; set; }

        /// <summary>
        /// Make the thing
        /// </summary>
        /// <returns>blank or error message</returns>
        string Craft(IEntity crafter);

        /// <summary>
        /// Render the blueprints to someone
        /// </summary>
        /// <returns>formatted list of components and requirements</returns>
        string RenderBlueprints(IEntity actor);
    }
}
