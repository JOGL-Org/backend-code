using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommunityEntityPermissionModel
    {
        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }
    }
}