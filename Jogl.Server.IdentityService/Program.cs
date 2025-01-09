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
using static Org.BouncyCastle.Math.EC.ECCurve;

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

var cert = await certClient.GetCertificateAsync(builder.Configuration["JWT:Cert-Name"]);

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
            RedirectUris = {  "https://citizenscience.nl:10003/oauth2/complete/jogl/" },
            PostLogoutRedirectUris = { "https://citizenscience.nl:10003/logout/" },
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
            },
            // Add these settings
            RequireConsent = false,
            AllowOfflineAccess = true,  // If you need refresh tokens
            AccessTokenLifetime = 3600,  // 1 hour
            AuthorizationCodeLifetime = 300  // 5 minutes
        }
        ])
         .AddInMemoryIdentityResources(new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            })
    .AddSigningCredential(new X509SigningCredentials(new X509Certificate2(cert.Value.Cer)))
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
