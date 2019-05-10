using NetMud.Data.Architectural.EntityBase;
using NetMud.DataStructure.Game;
using Newtonsoft.Json;
using NLua;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    [Serializable]
    public class RunningGame : EntityPartial, IRunningGame
    {
        public override T Template<T>()
        {
            return (T)Template<IGameTemplate>();
        }

        public override string TemplateName => Template<IGameTemplate>().Name;

        public IGameTemplate Game { get; set; }

        public IGameContext Context { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public Lua Engine { get; set; }

        public RunningGame()
        {
            Context = new GameContext();
        }

        public RunningGame(IGameTemplate game)
        {
            Context = new GameContext();
            Game = game;
            Engine = game.GetEngine();
        }

        public void NextTurn(string move)
        {

        }

        public string CurrentState()
        {
            return Context.State;
        }

        public override void SpawnNewInWorld()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new RunningGame()
            {
                TemplateId = TemplateId,
                Description = Description,
                Game = Game,
                Engine = Engine,
                Context = Context
            };
        }
    }
}
