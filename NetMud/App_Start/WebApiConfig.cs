using System.Web.Http;

namespace NetMud
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ApiModelDataReturnAjax",
                routeTemplate: "api/ClientDataApi/GetEntityModelView/{modelId}/{yaw}/{pitch}/{roll}",
                defaults: new { controller = "ClientDataApi", action = "GetEntityModelView" }
                );

            config.Routes.MapHttpRoute(
                name: "ApiModelPlanarData",
                routeTemplate: "api/ClientDataApi/GetDimensionalData/{id}",
                defaults: new { controller = "ClientDataApi", action = "GetDimensionalData" }
                );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
