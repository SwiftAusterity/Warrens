using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Web.WebSockets;
using NetMud.Authentication;
using NetMud.Websock;

namespace NetMud
{
    public class WebsocketHandler : IHttpHandler
    {
        public ApplicationUserManager UserManager { get; private set; }

        public WebsocketHandler()
        {
        }

        public WebsocketHandler(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public void ProcessRequest(HttpContext context)
        {
            UserManager = context.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(new Descriptor(UserManager));
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}