using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Lookup Data for mobile race
    /// </summary>
    public interface IRace : ICanConsume, ICanReproduce
    {
        string Name { get; set; }

        /// <summary>
        /// Material that is the blood
        /// </summary>
        HashSet<IInanimateTemplate> ButcherResults { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        ValueRange<short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        bool Aquatic { get; set; }

        /// <summary>
        /// The name used to describe a large gathering of this race
        /// </summary>    
        string CollectiveNoun { get; set; }
    }
}
