using GrpcGreeterServerCodeFirst.AI;
using GrpcGreeterServerCodeFirst.Services;
using Microsoft.AspNetCore.HttpLogging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ProtoBuf.Grpc.Server;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCodeFirstGrpc(config => { config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal; });

builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

builder.Services.AddControllers()
    .AddNewtonsoftJson(o =>
    {
        ((DefaultContractResolver)o.SerializerSettings.ContractResolver!).NamingStrategy = new DefaultNamingStrategy();
        o.SerializerSettings.Converters.Add(new StringEnumConverter(new DefaultNamingStrategy()));
    });

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddSwaggerGenNewtonsoftSupport();

builder.Services.AddApplicationInsights(builder.Configuration);
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.All;
    o.MediaTypeOptions.AddText("application/grpc-web", Encoding.UTF8);
    o.MediaTypeOptions.AddText("application/grpc", Encoding.UTF8);
    o.MediaTypeOptions.AddText("application/json", Encoding.UTF8);
});

var app = builder.Build();


app.UseHttpsRedirection();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IreckonU Match & Merge API");
});

app.UseStaticFiles();
app.UseCors();

app.UseHttpLogging(); // HttpLogging works fine, but dumps everything is separate entries to ILogger and is therefore useless for production use
app.UseRequestGrabber(); // Add request and response bodies to AI telemetry (enabled through config)


app.UseRouting();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // Must be added between UseRouting and UseEndpoints (https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-6.0#configure-grpc-web-in-aspnet-core)

app.UseEndpoints(endpoints => {
    endpoints.MapGrpcService<GreeterService>();
    endpoints.MapControllers();
});


app.Run();
