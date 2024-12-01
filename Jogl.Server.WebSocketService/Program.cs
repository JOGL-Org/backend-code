using Jogl.Server.Sockets;
using Jogl.Server.WebSocketService;
using Jogl.Server.ServiceBus;
using Jogl.Server.WebSocketService.Sockets;
using Jogl.Server.Configuration;
using Jogl.Server.DB.Extensions;
using Jogl.Server.DB.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("*")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
        });
});

//data access
builder.Services.AddScoped<IOperationContext, OperationContext>();
builder.Services.AddRepositories();

//sockets
builder.Services.AddSockets<JoglWebSocketGateway, IWebSocketGateway>();
builder.Services.AddHostedService<Service>();
//builder.Services.AddApplicationInsightsTelemetry();

//service bus
builder.Services.AddTransient<IServiceBusProxy, AzureServiceBusProxy>();

//add secrets
builder.Configuration.AddKeyVault();

var app = builder.Build();

//enable CORS
app.UseCors();

//configure websockets
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();


app.Run();