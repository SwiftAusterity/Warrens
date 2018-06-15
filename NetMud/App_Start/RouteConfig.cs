﻿using System.Web.Mvc;
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
                name: "SelectCharacterAjax",
                url: "GameAdmin/Player/SelectCharacter/{CurrentlySelectedCharacter}",
                defaults: new { controller = "Player", action = "SelectCharacter" }
                );

            routes.MapRoute(
                name: "Room Add Modal",
                url: "GameAdmin/Room/Add/{localeId}",
                defaults: new { controller = "Room", action = "Add" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Pathway Add Edit Descriptive Modal",
                url: "GameAdmin/Pathway/AddEditDescriptive/{id}/{descriptiveType}/{phrase}",
                defaults: new { controller = "Pathway", action = "AddEditDescriptive", phrase = "" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Pathway Add Modal",
                url: "GameAdmin/Pathway/Add/{id}/{originRoomId}/{destinationRoomId}/{degreesFromNorth}",
                defaults: new { controller = "Pathway", action = "Add", degreesFromNorth = UrlParameter.Optional },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Pathway Edit Modal",
                url: "GameAdmin/Pathway/Edit/{id}/{originRoomId}/{destinationRoomId}",
                defaults: new { controller = "Pathway", action = "Edit" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Zone Add Edit Descriptive Modal",
                url: "GameAdmin/Zone/AddEditDescriptive/{id}/{descriptiveType}/{phrase}",
                defaults: new { controller = "Zone", action = "AddEditDescriptive", phrase = "" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );


            routes.MapRoute(
                name: "Room Add Edit Descriptive Modal",
                url: "GameAdmin/Room/AddEditDescriptive/{id}/{descriptiveType}/{phrase}",
                defaults: new { controller = "Room", action = "AddEditDescriptive", phrase = "" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "NPC Add Edit Descriptive Modal",
                url: "GameAdmin/NPC/AddEditDescriptive/{id}/{descriptiveType}/{phrase}",
                defaults: new { controller = "NPC", action = "AddEditDescriptive", phrase = "" },
                namespaces: new string[] { "NetMud.Controllers.GameAdmin" }
            );

            routes.MapRoute(
                name: "Inanimate Add Edit Descriptive Modal",
                url: "GameAdmin/Inanimate/AddEditDescriptive/{id}/{descriptiveType}/{phrase}",
                defaults: new { controller = "Inanimate", action = "AddEditDescriptive", phrase = "" },
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
