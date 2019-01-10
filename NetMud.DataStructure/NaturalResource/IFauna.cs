using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.NaturalResource
{
    /// <summary>
    /// Animal spawns
    /// </summary>
    public interface IFauna : INaturalResource
    {
        /// <summary>
        /// What we're spawning
        /// </summary>
        IRace Race { get; set; }

        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        
        int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        
        int PopulationHardCap { get; set; }
    }
}
