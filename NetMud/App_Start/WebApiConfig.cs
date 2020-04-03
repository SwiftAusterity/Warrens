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

            #region "AdminAPI"
            #endregion

            #region "LexicaApi"
            config.Routes.MapHttpRoute(
                name: "api_describe",
                routeTemplate: "api/lexicaapi/Describe",
                defaults: new { controller = "LexicaApi", action = "Describe" }
            );

            #endregion

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
