using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Action
{
    /// <summary>
    /// Pre-requisites to using an action
    /// </summary>
    [Serializable]
    public class ActionCriteria : IActionCriteria
    {
        /// <summary>
        /// Target type of the criteria, what are we checking against
        /// </summary>
        public ActionTarget Target { get; set; }

        /// <summary>
        /// Cheaty way of doing this - only affects entities with a backingdata of this ID, the actionTarget tells us the type
        /// </summary>
        [UIHint("KeyedDataList")]
        public long AffectsMemberId { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// The value range of the quality we're checking for
        /// </summary>
        public ValueRange<int> ValueRange { get; set; }

        public ActionCriteria()
        {
            AffectsMemberId = -1;
            ValueRange = new ValueRange<int>(0, 0);
        }

        /// <summary>
        /// Get the keyed data member out from the ID
        /// </summary>
        /// <typeparam name="T">The expected type of the member</typeparam>
        /// <returns>the member</returns>
        public T GetMember<T>() where T : IKeyedData
        {
            switch(Target)
            {
                case ActionTarget.Item:
                    if (typeof(T) == typeof(IInanimateTemplate))
                        return TemplateCache.Get<T>(AffectsMemberId);
                    break;
                case ActionTarget.NPC:
                    if (typeof(T) == typeof(INonPlayerCharacterTemplate))
                        return TemplateCache.Get<T>(AffectsMemberId);
                    break;
                case ActionTarget.Tile:
                    if (typeof(T) == typeof(ITileTemplate))
                        return TemplateCache.Get<T>(AffectsMemberId);
                    break;
            }

            return default(T);
        }

        public object Clone()
        {
            ActionCriteria returnValue = new ActionCriteria
            {
                Quality = Quality,
                Target = Target,
                ValueRange = ValueRange
            };

            return returnValue;
        }
    }
}
