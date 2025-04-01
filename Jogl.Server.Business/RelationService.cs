using Jogl.Server.Data;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class RelationService : IRelationService
    {
        private readonly IRelationRepository _relationRepository;
        private readonly IMembershipRepository _membershipRepository;

        public RelationService(IRelationRepository relationRepository, IMembershipRepository membershipRepository)
        {
            _relationRepository = relationRepository;
            _membershipRepository = membershipRepository;
        }

        public List<string> ListCommunityEntityIdsForNode(string nodeId)
        {
            var allRelations = _relationRepository.Query(r => true).ToList();

            var directLinkCommunityIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Node && r.SourceCommunityEntityType == CommunityEntityType.Workspace && r.TargetCommunityEntityId == nodeId)
               .Select(r => r.SourceCommunityEntityId);

            var indirectLinkIds = allRelations.Where(r => r.TargetCommunityEntityType == CommunityEntityType.Workspace && directLinkCommunityIds.Contains(r.TargetCommunityEntityId))
                .Select(r => r.SourceCommunityEntityId);

            return directLinkCommunityIds.Concat(indirectLinkIds).Concat([nodeId])
              .Distinct()
              .ToList();
        }

        public List<string> ListUserIdsForNode(string nodeId)
        {
            var entityIds = ListCommunityEntityIdsForNode(nodeId);
            var memberships = _membershipRepository.Query(m => entityIds.Contains(m.CommunityEntityId)).ToList();

            return memberships.Select(m => m.UserId).Distinct().ToList();
        }
    }
}