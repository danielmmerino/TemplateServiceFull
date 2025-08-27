using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

builder.Services.AddReverseProxy().LoadFromMemory(
    routes: new[]
    {
        new Yarp.ReverseProxy.Configuration.RouteConfig
        {
            RouteId = "template",
            ClusterId = "template-cluster",
            Match = new() { Path = "/template/{**catch-all}" }
        }
    },
    clusters: new[]
    {
        new Yarp.ReverseProxy.Configuration.ClusterConfig
        {
            ClusterId = "template-cluster",
            Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
            {
                ["d1"] = new() { Address = "http://template-service:8080/" }
            }
        }
    }
);

var app = builder.Build();
app.MapReverseProxy();
app.MapGet("/", () => "Gateway up");
app.Run();
