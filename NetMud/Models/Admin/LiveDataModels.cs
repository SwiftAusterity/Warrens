using NetMud.Authentication;


namespace NetMud.Models.Admin
{
    public abstract class LiveEntityViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }
    }
}