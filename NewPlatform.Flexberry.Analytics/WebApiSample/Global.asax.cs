namespace NewPlatform.Flexberry.Analytics.WebApiSample
{
    using System.Web.Http;
    using NewPlatform.Flexberry.Analytics.WebAPI;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(ReportApiConfig.Register);
        }
    }
}