namespace Jogl.Server.Data
{
    public enum EmailRecordType { Mention, Notification, Share }
    public class EmailRecord : Entity
    {
        public EmailRecordType Type { get; set; }
        public string ObjectId { get; set; }
        public string UserId { get; set; }
        public string Key { get; set; }
    }
}