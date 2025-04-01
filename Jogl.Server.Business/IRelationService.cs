namespace Jogl.Server.Business
{
    public interface IRelationService
    {
        List<string> ListCommunityEntityIdsForNode(string nodeId);
        List<string> ListUserIdsForNode(string nodeId);
    }
}