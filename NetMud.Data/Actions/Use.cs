using NetMud.DataStructure.Action;
using System;

namespace NetMud.Data.Action
{
    /// <summary>
    /// Interactions for players against tiles
    /// </summary>
    [Serializable]
    public class Use : Action, IUse
    {
        public Use() : base() { }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Use
            {
                FoleyUri = FoleyUri,
                Name = Name,
                StaminaCost = StaminaCost,
                ToLocalMessage = ToLocalMessage,
                ToActorMessage = ToActorMessage,
                HealthCost = HealthCost,
                Criteria = Criteria,
                Results = Results
            };
        }
    }
}
