using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum UserStatus { Pending, Verified }
    
    [BsonIgnoreExtraElements]
    public class User : FeedEntity
    {
        public string FirstName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } set { } }
        public string LastName { get; set; }
        public string Username { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string BannerId { get; set; }
        public string AvatarId { get; set; }
        public UserStatus Status { get; set; }
        public string StatusText { get; set; }
        public string ShortBio { get; set; }
        [RichText]
        public string Bio { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public List<Link> Links { get; set; }
        public List<string> Skills { get; set; }
        public List<string> Interests { get; set; }
        public List<string> Assets { get; set; }
        public string OrcidId { get; set; }
        public bool Newsletter { get; set; }
        public bool ContactMe { get; set; }
        [Obsolete]
        public string FeedId { get; set; }
        public UserNotificationSettings NotificationSettings { get; set; }
        public List<UserExperience>? Experience { get; set; }
        public List<UserEducation>? Education { get; set; }
        public UserExternalAuth Auth { get; set; }
        public DateTime? NotificationsReadUTC { get; set; }
        public string Language { get; set; }

        [BsonIgnore]
        public int FollowedCount { get; set; }
        [BsonIgnore]
        public int FollowerCount { get; set; }
        [BsonIgnore]
        public bool UserFollows { get; set; }
        [BsonIgnore]
        public int ProjectCount { get; set; }
        [BsonIgnore]
        public int CommunityCount { get; set; }
        [BsonIgnore]
        public int NodeCount { get; set; }
        [BsonIgnore]
        public int OrganizationCount { get; set; }
        [BsonIgnore]
        public int NeedCount { get; set; }
        [BsonIgnore]
        public List<Organization> Organizations { get; set; }

        public override string FeedTitle => FullName;

        public override FeedType FeedType => FeedType.User;
    }
}