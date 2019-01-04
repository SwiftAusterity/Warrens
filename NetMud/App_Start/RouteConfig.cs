using System.Web.Mvc;
using System.Web.Routing;

namespace NetMud
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ModalErrorOrClose",
                url: "GameAdmin/ModalErrorOrClose/{Message}",
                defaults: new { controller = "GameAdmin", action = "ModalErrorOrClose", Message = UrlParameter.Optional },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
              );

            routes.MapRoute(
                name: "Content ApprovalDenial",
                url: "GameAdmin/ContentApproval/ApproveDeny/{approvalId}/{authorizeApproval}/{denialId}/{authorizeDenial}",
                defaults: new { controller = "ContentApproval", action = "ApproveDeny", approvalId = UrlParameter.Optional, authorizeApproval = UrlParameter.Optional, denialId = UrlParameter.Optional, authorizeDenial = UrlParameter.Optional },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "NetMud.Controllers" }
            );
        }
    }
}
