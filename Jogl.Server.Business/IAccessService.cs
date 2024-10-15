using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public enum ActionAllowResult { ForbiddenSilent, Forbidden, Allowed }
    public enum Action { List, Read, Contribute, ManageMembers, Delete }
    public interface IAccessService
    {
        string GetUserAccessLevel(Membership membership, Invitation invitation = null);
        JoiningRestrictionLevel? GetUserJoiningRestrictionLevel(Membership membership, string currentUserId, CommunityEntity entity);
    }
}