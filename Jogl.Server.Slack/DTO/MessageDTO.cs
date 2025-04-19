namespace Jogl.Server.Slack.DTO
{
    public record MessageDTO(string Id, bool FromUser, string Text)
    {
    }
}