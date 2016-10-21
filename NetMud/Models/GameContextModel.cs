using NetMud.Authentication;

namespace NetMud.Models
{
    public class GameContextModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }
    }
}