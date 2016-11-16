namespace NetMud.DataStructure.Base.Skills
{
    /// <summary>
    /// Skill tree leaves
    /// </summary>
    public interface ISkillLeaf
    {
        /// <summary>
        /// The raw value of this skill leaf
        /// </summary>
        int Value { get; set; }

        /// <summary>
        /// The weighted value of this leaf
        /// </summary>
        /// <returns>the weighted value of this leaf</returns>
        double WeightedValue();
    }
}
