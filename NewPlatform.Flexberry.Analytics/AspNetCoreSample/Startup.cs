namespace NewPlatform.Flexberry.Analytics.AspNetCoreSample
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NewPlatform.Flexberry.Analytics.Abstractions;
    using NewPlatform.Flexberry.Analytics.Pentaho;
    using NewPlatform.Flexberry.Analytics.WebAPI;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            int timeout = 0;
            int.TryParse(Configuration["DefaultTimeout"], out timeout);
            services.AddScoped<IReportManager, PentahoReportManager>((_) => new PentahoReportManager(
                Configuration["ReportServiceEndpoint"],
                Configuration["PentahoReportLogin"],
                Configuration["PentahoReportPassword"],
                timeout));

            services.AddControllers()
                .AddApplicationPart(typeof(ReportController).Assembly) // необходимо чтобы добавился контроллер из соседней сборки
                .AddNewtonsoftJson(); // необходимо чтобы работал биндинг [FromBody] JObject
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
