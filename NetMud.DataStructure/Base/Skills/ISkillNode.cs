using System.Collections.Generic;
namespace NetMud.DataStructure.Base.Skills
{
    /// <summary>
    /// Skill tree Nodes (contains nodes and leaves)
    /// </summary>
    public interface ISkillNode
    {
        /// <summary>
        /// Children nodes
        /// </summary>
        IEnumerable<ISkillNode> Nodes { get; set; }

        /// <summary>
        /// Children Leaves
        /// </summary>
        IEnumerable<ISkillLeaf> Leaves { get; set; }

        /// <summary>
        /// Gets all the child leaves regardless of depth
        /// </summary>
        /// <returns>all child leaves under this node</returns>
        IEnumerable<ISkillLeaf> GetAllChildLeaves();

        /// <summary>
        /// Gets the effective skill value of this node
        /// </summary>
        /// <returns>the effective skill value of the node</returns>
        double EffectiveSkillValue();
    }
}
