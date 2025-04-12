//using SlackNet;
//using Microsoft.AspNetCore.Mvc;

//public class SlackOAuthController : Controller
//{
//    private readonly ISlackApiClient _slack;
//    private readonly IInstallationStore _installationStore;

//    public SlackOAuthController(ISlackApiClient slack, IInstallationStore installationStore)
//    {
//        _slack = slack;
//        _installationStore = installationStore;
//    }

//    [HttpGet("/slack/oauth")]
//    public async Task<IActionResult> OAuthCallback(string code)
//    {
//        // Exchange the temporary code for tokens
//        var response = await _slack.OAuthV2.Access("293774504805.8714801217828", "a925e8ac4b31da03e16e8eaaa8ca3ea5", code, "authorization_code", "https://frontendjogl.azurewebsites.net/callback", null, CancellationToken.None);

//        // Store the installation
//        await _installationStore.SaveInstallation(new WorkspaceInstallation
//        {
//            TeamId = response.TeamId,
//            TeamName = response.TeamName,
//            AccessToken = response.AccessToken,
//            BotUserId = response.BotUserId,
//            InstalledAt = DateTime.UtcNow
//        });

//        return Redirect("https://your-app.com/success");
//    }
//}