using Jogl.Server.Configuration;
using Jogl.Server.SlackAgentAPI.Handler;
using Microsoft.OpenApi.Models;
using SlackNet.Events;
using Jogl.Server.Slack.Extensions;
using Jogl.Server.ServiceBus.Extensions;
using Jogl.Server.DB.Extensions;
using Jogl.Server.AI.Extensions;

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JOGL Slack API",
        Version = "v1"
    });
    config.SupportNonNullableReferenceTypes();
    config.EnableAnnotations();
});

builder.Configuration.AddKeyVault();
builder.Services.AddServiceBus();
builder.Services.AddRepositories();
builder.Services.AddAI();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSlack(builder.Configuration, (c) =>
{
    c.RegisterEventHandler<MessageEvent, MessageHandler>();
    //    c.RegisterEventHandler<TeamJoin, TeamJoinHandler>();
    //c.RegisterEventHandler<BotAdded, BotAddedHandler>();
});

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseSlack();

app.Run();