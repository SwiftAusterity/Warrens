using NetMud.DataStructure.Base.Skills;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity can access skill and spell commands
    /// </summary>
    public interface IHasSkills
    {
        IEnumerable<ISkillNode> SkillTrees { get; set; }

        /// <summary>
        /// Gets a skill leaf by the full abbrevation
        /// </summary>
        /// <param name="abbreviationChain">the abbreviation chain</param>
        /// <returns>a skill leaf matching that abbreviation</returns>
        ISkillLeaf GetSkillLeafByAbbreviation(string abbreviationChain);
    }
}
