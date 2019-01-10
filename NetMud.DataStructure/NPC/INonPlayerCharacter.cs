using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// NPC entity class
    /// </summary>
    public interface INonPlayerCharacter : IMobile, INonPlayerCharacterFramework, ISpawnAsMultiple, IThink, IAmAMerchant, IAmATeacher
    {
    }
}
