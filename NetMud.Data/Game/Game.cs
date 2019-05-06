using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Game;
using Newtonsoft.Json;
using NLua;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    [Serializable]
    public class Game : TemplatePartial, IGame
    {
        [JsonIgnore]
        [ScriptIgnore]
        public Type EntityClass => typeof(RunningGame);

        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.Admin;

        public ValueRange<short> NumberOfPlayers { get; set; }

        public string Description { get; set; }

        public int AverageDuration { get; set; }

        public bool HighScoreboard { get; set; }

        public bool PublicReplay { get; set; }

        public int TurnDuration { get; set; }

        public Game()
        {
            NumberOfPlayers = new ValueRange<short>(2, 4);
            AverageDuration = 30;
            HighScoreboard = true;
            PublicReplay = true;
            TurnDuration = 5;
        }

        public Lua GetEngine()
        {
            return new Lua();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
