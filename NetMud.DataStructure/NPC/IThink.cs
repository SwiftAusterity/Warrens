using NetMud.DataStructure.NPC.IntelligenceControl;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Indicates an entity is subject to artificial intelligence triggers and can think for itself
    /// </summary>
    public interface IThink
    {
        IBrain Hypothalamus { get; set; }

        void DoTheThing(Motivator motivator);
    }
}
