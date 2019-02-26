using NetMud.Authentication;

namespace NetMud.Models
{
    public interface IBaseViewModel
    {
        ApplicationUser AuthedUser { get; set; }
    }
}