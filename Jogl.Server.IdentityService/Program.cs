using Duende.IdentityServer.Models;
using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Jogl.Server.Auth;
using Jogl.Server.DB;
using Jogl.Server.Configuration;
using Jogl.Server.Auth.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<IAuthChallengeService, AuthChallengeService>();
builder.Services.AddTransient<IAuthService, AuthService>();

builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Configuration.AddKeyVault();

var certClient = new CertificateClient(
    new Uri(builder.Configuration["Azure:KeyVault:URL"]),
    new DefaultAzureCredential()
);

var cert = certClient.DownloadCertificate(builder.Configuration["JWT:Cert-Name"]);
builder.Services.AddIdentityServer(options =>
{
    //options.Events.RaiseErrorEvents = true;
    //options.Events.RaiseInformationEvents = true;
    //options.Events.RaiseFailureEvents = true;
    //options.Events.RaiseSuccessEvents = true;
    //options.EmitStaticAudienceClaim = true;
})
        .AddInMemoryClients([
        new Client
        {
            ClientId = builder.Configuration["OAuth:CitizenScience:Id"],
            ClientSecrets = { new Secret (builder.Configuration["OAuth:CitizenScience:Secret"].Sha256()) },
            ClientName = builder.Configuration["OAuth:CitizenScience:Name"],
            RequirePkce = false,
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { builder.Configuration["OAuth:CitizenScience:RedirectUriLogin"] },
            PostLogoutRedirectUris ={ builder.Configuration["OAuth:CitizenScience:RedirectUriLogout"] },
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
            },
            RequireConsent = false,
            AllowOfflineAccess = true,
            AccessTokenLifetime = 3600,
            AuthorizationCodeLifetime = 300
        }
        ])
         .AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            })
    .AddSigningCredential(new X509SigningCredentials(new X509Certificate2(cert.Value)))
    .AddDeveloperSigningCredential()
    .AddProfileService<OAuthProfileService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityServer();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
