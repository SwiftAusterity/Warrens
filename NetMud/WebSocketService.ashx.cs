using NetMud.Authentication;
using NetMud.Websock;

namespace NetMud
{
    /// <summary>
    /// Summary description for WebSocketService
    /// </summary>
    public class WebSocketService : IHttpHandler
    {

        public ApplicationUserManager UserManager { get; private set; }

        public WebSocketService()
        {
        }

        public WebSocketService(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public void ProcessRequest(HttpContext context)
        {
            UserManager = context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(new Descriptor(UserManager));
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}