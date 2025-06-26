using AutoMapper;
using Jogl.Server.API.Converters;
using Jogl.Server.API.Mapping;
using Jogl.Server.API.Middleware;
using Jogl.Server.Auth;
using Jogl.Server.OpenAlex;
using Jogl.Server.Orcid;
using Jogl.Server.PubMed;
using Jogl.Server.GoogleAuth;
using Jogl.Server.SemanticScholar;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Polly;
using Polly.Retry;
using Jogl.Server.LinkedIn;
using Jogl.Server.API.Services;
using Jogl.Server.Images;
using Jogl.Server.Documents;
using Jogl.Server.Configuration;
using Jogl.Server.Search.Extensions;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using System.Security.Cryptography.X509Certificates;
using Jogl.Server.DB.Extensions;
using Jogl.Server.Lix;
using Jogl.Server.Business.Extensions;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SchemaFilter<SchemaFilter>();
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JOGL API",
        Version = "v1"
    });
    config.SupportNonNullableReferenceTypes();
    config.EnableAnnotations();
    config.ParameterFilter<QueryArrayParameterFilter>();
});

// Program.cs
builder.Services.AddTransient<TelemetryMiddleware>();
//api services
builder.Services.AddTransient<IVerificationService, CaptchaVerificationService>();
builder.Services.AddTransient<IContextService, ContextService>();
//auth services

builder.Services.AddBusiness();
builder.Services.AddRepositories();
builder.Services.AddSearch();
builder.Services.AddInitialization();

builder.Services.AddTransient<IGoogleFacade, GoogleFacade>();
builder.Services.AddTransient<ILinkedInFacade, LinkedInFacade>();
builder.Services.AddTransient<ILixFacade, LixFacade>();

//images
builder.Services.AddTransient<IConversionService, ConversionService>();

//documents
builder.Services.AddTransient<IDocumentConverter, DocumentConverter>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IOperationContext, HttpOperationContext>();
builder.Services.AddSingleton(provider => new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MappingProfiles(provider.GetService<IHttpContextAccessor>()));
}).CreateMapper());

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(new LowercaseJsonNamingPolicy()));
    //options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
});

//add keys
builder.Configuration.AddKeyVault();
builder.Services.AddApplicationInsightsTelemetry();

var certClient = new CertificateClient(
    new Uri(builder.Configuration["Azure:KeyVault:URL"]),
    new DefaultAzureCredential()
);

var cert = await certClient.GetCertificateAsync(builder.Configuration["JWT:Cert-Name"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new X509SecurityKey(new X509Certificate2(cert.Value.Cer)),
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

var app = builder.Build();

// initialize licenses
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(app.Configuration["Syncfusion:License"]);

// initialize DB
await app.InitializeDBAsync();

//enable CORS
app.UseCors();

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

//enable custom middleware
app.UseMiddleware<JObjectMiddleware>();
app.UseMiddleware<ContextMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<TelemetryMiddleware>();

app.MapControllers();
app.Run();
