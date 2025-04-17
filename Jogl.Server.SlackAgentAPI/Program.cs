using Jogl.Server.Configuration;
using Jogl.Server.SlackAgentAPI.Handler;
using Microsoft.OpenApi.Models;
using SlackNet.AspNetCore;
using SlackNet.Extensions.DependencyInjection;
using SlackNet.Events;
using Jogl.Server.Search;
using Jogl.Server.DB.Extensions;
using Jogl.Server.Business;
using Jogl.Server.AI.Agent.Extensions;
using Jogl.Server.Business.Extensions;

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

//data access
builder.Services.AddBusiness();
builder.Services.AddRepositories();

builder.Services.AddScoped<IRelationService, RelationService>();
builder.Services.AddSingleton<ISearchService, AzureSearchService>();
builder.Services.AddAIAgent();

//add secrets
builder.Configuration.AddKeyVault();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSlackNet(c =>
{
    c.UseAppLevelToken(builder.Configuration["Slack:AppLevelToken"]);
    c.UseSigningSecret(builder.Configuration["Slack:SigningSecret"]);
    c.RegisterEventHandler<MessageEvent, MessageHandler>();
    c.RegisterEventHandler<MemberJoinedChannel, ChannelHandler>();
    c.RegisterEventHandler<BotAdded, BotAddedHandler>();
});
var app = builder.Build();

//enable CORS
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();
app.UseSlackNet(c =>
{
    c.UseSocketMode(true);
});

app.MapControllers();
app.Run();