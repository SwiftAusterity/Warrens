using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Methods and properties for instructor NPCs
    /// </summary>
    public interface IAmATeacher : IHaveSkillsToTeach
    {
        /// <summary>
        /// Indicates whether this is actually a teacher who can teach things
        /// </summary>
        /// <returns>if they sell things (but not if they have stock)</returns>
        bool DoITeachThings();

        /// <summary>
        /// Price check on teaching qualities
        /// </summary>
        /// <param name="name">The name of the quality</param>
        /// <param name="level">The level to teach to</param>
        /// <returns>the price, -1 indicates it wont be taught</returns>
        int InstructionPriceCheck(string qualityName, int level);

        /// <summary>
        /// Render the list of skills to teach to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the inventory display</returns>
        string RenderInstructionList(IEntity customer);

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        string Instruct(IMobile customer, string useName, int price);

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        string Instruct(IMobile customer, string qualityName, int level, int price);
    }

}
