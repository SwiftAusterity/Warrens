using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NLua;

namespace NetMud.DataStructure.Game
{
    public interface IGameTemplate : ITemplate
    {
        ValueRange<short> NumberOfPlayers { get; set; }

        MarkdownString Description { get; set; }

        int AverageDuration { get; set; }

        int TurnDuration { get; set; }

        bool HighScoreboard { get; set; }

        bool PublicReplay { get; set; }

        Lua GetEngine();
    }
}
