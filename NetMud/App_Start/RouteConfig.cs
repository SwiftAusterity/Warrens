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
                name: "SelectCharacterAjax",
                url: "GameAdmin/Player/SelectCharacter/{CurrentlySelectedCharacter}",
                defaults: new { controller = "Player", action = "SelectCharacter" }
                );

            routes.MapRoute(
                name: "Pathway Add Modal",
                url: "GameAdmin/Pathway/Add/{id}/{originRoomId}/{destinationRoomId}",
                defaults: new { controller = "Pathway", action = "Add" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Pathway Edit Modal",
                url: "GameAdmin/Pathway/Edit/{id}/{originRoomId}/{destinationRoomId}",
                defaults: new { controller = "Pathway", action = "Edit" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "GameAdminSubControllers",
                url: "GameAdmin/{controller}/{action}/{id}",
                defaults: new { controller = "Player", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "GameAdminBaseController",
                url: "GameAdmin/{action}/{id}",
                defaults: new { controller = "GameAdmin", action = "Index", id = UrlParameter.Optional },
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
