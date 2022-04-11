using GrpcGreeterServerCodeFirst.AI;
using GrpcGreeterServerCodeFirst.Services;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ProtoBuf.Grpc.Server;

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

app.UseRequestGrabber(); // Add request and response bodies to AI telemetry (enabled through config)

app.UseRouting();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // Must be added between UseRouting and UseEndpoints (https://docs.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-6.0#configure-grpc-web-in-aspnet-core)

app.UseEndpoints(endpoints => {
    endpoints.MapGrpcService<GreeterService>();
    endpoints.MapControllers();
});


app.Run();
