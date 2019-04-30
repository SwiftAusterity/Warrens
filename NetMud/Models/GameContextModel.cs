using NetMud.Authentication;

namespace NetMud.Models
{
    public class GameContextModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }
    }
}