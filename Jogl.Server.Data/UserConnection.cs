namespace Jogl.Server.Data
{
    public enum UserConnectionStatus { Pending, Accepted, Rejected }

    public class UserConnection : Entity
    {
        public string FromUserId { get; set; }

        public string ToUserId { get; set; }

        public UserConnectionStatus Status { get; set; }
    }
}