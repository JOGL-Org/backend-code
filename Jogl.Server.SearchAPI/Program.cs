using Jogl.Server.Business.Extensions;
using Jogl.Server.Configuration;
using Jogl.Server.Search.Extensions;
using Jogl.Server.SearchAPI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
    config.EnableAnnotations();

    config.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Enter your API key in the text input below.",
        Name = ExternalIdResult.API_KEY_HEADER_NAME,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddBusiness();
builder.Services.AddSingleton<IContextService, ContextService>();
builder.Configuration.AddKeyVault();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ExternalIdResult>("ApiKey", options => { });

builder.Services.AddSearch();
builder.Services.AddAuthorization();

var app = builder.Build();

// configure swaggger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
