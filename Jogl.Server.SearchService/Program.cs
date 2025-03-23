using Jogl.Server.Configuration;
using Jogl.Server.DB.Extensions;
using Jogl.Server.DB.Context;
using Jogl.Server.Search;
using Microsoft.OpenApi.Models;
using Jogl.Server.AI;

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
        Title = "JOGL Search API",
        Version = "v1"
    });
    config.SupportNonNullableReferenceTypes();
    //   config.EnableAnnotations();
});

//data access
builder.Services.AddScoped<IOperationContext, OperationContext>();
builder.Services.AddRepositories();

builder.Services.AddScoped<ISearchService, AzureSearchService>();
builder.Services.AddScoped<IAIService, ClaudeAIService>();
//builder.Services.AddApplicationInsightsTelemetry();

//add secrets
builder.Configuration.AddKeyVault();

var app = builder.Build();

//enable CORS
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.MapControllers();
app.Run();