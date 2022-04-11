using Microsoft.ApplicationInsights;

namespace GrpcGreeterServerCodeFirst.AI
{
    public static class Extensions
    {
        public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton((sp) => new RequestGrabber(sp.GetRequiredService<TelemetryClient>(), configuration)); ;
            return services;
        }

        public static IApplicationBuilder UseRequestGrabber(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestGrabber>();
            return app;
        }
    }
}
