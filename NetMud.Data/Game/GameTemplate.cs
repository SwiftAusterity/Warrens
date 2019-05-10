using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using NetMud.DataStructure.Game;
using Newtonsoft.Json;
using NLua;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    [Serializable]
    public class GameTemplate : TemplatePartial, IGameTemplate
    {
        [JsonIgnore]
        [ScriptIgnore]
        public Type EntityClass => typeof(RunningGame);

        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.Admin;

        [Display(Name = "Number Of Players", Description = "What number of players this supports.")]
        [UIHint("ValueRangeShort")]
        public ValueRange<short> NumberOfPlayers { get; set; }

        [StringDataIntegrity("Body must be between 20 and 2000 characters", 20, 2000)]
        [MarkdownStringLengthValidator(ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 20)]
        [Display(Name = "Body", Description = "The body content of the entry.")]
        [DataType("Markdown")]
        [Required]
        [MarkdownBinder]
        public MarkdownString Description { get; set; }

        [Display(Name = "Average Duration", Description = "Recommended average duration for a full game.")]
        [DataType(DataType.Text)]
        public int AverageDuration { get; set; }

        [Display(Name = "Scoreboard", Description = "Whether to make a high scores board available for public display.")]
        [UIHint("Boolean")]
        public bool HighScoreboard { get; set; }

        [Display(Name = "Replays", Description = "Whether or not replays of this game will be available for public viewing.")]
        [UIHint("Boolean")]
        public bool PublicReplay { get; set; }

        [Display(Name = "Turn Duration", Description = "The maximum length for a single turn in minutes.")]
        [DataType(DataType.Text)]
        public int TurnDuration { get; set; }

        public GameTemplate()
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
