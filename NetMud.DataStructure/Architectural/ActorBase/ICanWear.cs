using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Entity can equip armor/clothing
    /// </summary>
    public interface ICanWear
    {
        /// <summary>
        /// Where can you wear things, [Name, [height, width, length]]
        /// </summary>
        Dictionary<string, Dimensions> ValidEquipmentSlots { get; set; }

        /// <summary>
        /// Current equipment worn
        /// </summary>
        Dictionary<string, IEntity> Equipped { get; set; }

        /// <summary>
        /// Can this be worn at slotName
        /// </summary>
        /// <param name="equipment">the thing to wear</param>
        /// <param name="slotName">the slot to wear it on</param>
        /// <returns>error message</returns>
        string CanBeWorn(IEntity equipment, string slotName);

        /// <summary>
        /// Put something on
        /// </summary>
        /// <param name="equipment">the thing to wear</param>
        /// <param name="slotName">the slot to wear it on</param>
        /// <returns>error message</returns>
        string Equip(IEntity equipment, string slotName);

        /// <summary>
        /// Remove something from a slot
        /// </summary>
        /// <param name="slotName">the slot to wear it on</param>
        /// <returns>error message</returns>
        string DeEquip(string slotName);
    }
}
