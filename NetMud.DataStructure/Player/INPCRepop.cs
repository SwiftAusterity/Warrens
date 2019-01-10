using NetMud.DataStructure.NPC;

namespace NetMud.DataStructure.Player
{
    public interface INPCRepop
    {
        INonPlayerCharacterTemplate NPC { get; set; }
        short Amount { get; set; }
    }

}
