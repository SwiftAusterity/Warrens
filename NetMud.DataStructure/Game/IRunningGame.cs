using NetMud.DataStructure.Architectural.EntityBase;
using NLua;

namespace NetMud.DataStructure.Game
{
    public interface IRunningGame : IEntity
    {
        IGame Game { get; set; }

        IGameContext Context { get; set; }

        Lua Engine { get; set; }

        void NextTurn(string move);

        string CurrentState();
    }
}
