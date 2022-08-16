namespace NewPlatform.Flexberry.Analytics.WebApiSample
{
    using System.Web.Http;
    using NewPlatform.Flexberry.AspNet.WebApi.Cors;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
            config.EnableCors(new DynamicCorsPolicyProvider(true));

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}