using NetMud.Authentication;

namespace NetMud.Models
{
    public interface IBaseViewModel
    {
        ApplicationUser authedUser { get; set; }
    }
}