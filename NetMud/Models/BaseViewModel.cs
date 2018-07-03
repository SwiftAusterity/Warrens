using NetMud.Authentication;

namespace NetMud.Models
{
    public interface BaseViewModel
    {
        ApplicationUser authedUser { get; set; }
    }
}