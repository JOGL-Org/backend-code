using Jogl.Server.Configuration;
using Jogl.Server.WhatsApp.Extensions;
using Jogl.Server.ServiceBus.Extensions;
using Twilio.AspNet.Core;
using Jogl.Server.AI.Extensions;
using Jogl.Server.DB.Extensions;

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

builder.Configuration.AddKeyVault();
builder.Services.AddControllers();
builder.Services.AddServiceBus();
builder.Services.AddRepositories();
builder.Services.AddAI();
builder.Services.AddWhatsApp(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<TwilioRequestValidationOptions>(options =>
{
    options.AuthToken = builder.Configuration["Twilio:AuthToken"];
});

var app = builder.Build();

app.MapControllers();
app.UseRouting();
app.UseCors();

app.Run();