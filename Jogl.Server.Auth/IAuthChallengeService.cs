namespace Jogl.Server.Auth
{
    public interface IAuthChallengeService
    {
        string GetChallenge(string key);
    }
}