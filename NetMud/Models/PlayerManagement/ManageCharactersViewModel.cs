using NetMud.Authentication;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.PlayerManagement
{
    public class ManageCharactersViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IGender> ValidGenders { get; set; }

        [UIHint("PlayerTemplate")]
        public IPlayerTemplate NewCharacter { get; set; }
    }


    public class AddEditCharacterViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditCharacterViewModel()
        {
        }

        [UIHint("PlayerTemplate")]
        public IPlayerTemplate DataObject { get; set; }
        public IEnumerable<IRace> ValidRaces { get; set; }
        public IEnumerable<IGender> ValidGenders { get; set; }
    }

}