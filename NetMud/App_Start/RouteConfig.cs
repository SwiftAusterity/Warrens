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
                name: "GameAdminSelectCharacterAjax",
                url: "GameAdmin/SelectCharacter/{CurrentlySelectedCharacter}",
                defaults: new { controller = "GameAdmin", action = "SelectCharacter" }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
