using Jogl.Server.Data;

namespace Jogl.Server.API.Model
{
    public class JoiningRestrictionLevelSettingModel
    {
        public string CommunityEntityId { get; set; }
        public JoiningRestrictionLevel Level { get; set; }
    }
}