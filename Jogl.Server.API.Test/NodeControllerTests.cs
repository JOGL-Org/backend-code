using Jogl.Server.API.Controllers;
using Jogl.Server.API.Model;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Moq;
using System.Linq.Expressions;

namespace Jogl.Server.API.Test
{
    public class NodeControllerTests : CommunityEntityControllerTestBase<NodeModel>
    {
        private NodeController _nodeController;
        private FeedController _feedController;
        private DocumentController _documentController;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _serviceCollection.AddTransient<NodeController>();
            _serviceCollection.AddTransient<FeedController>();
            _serviceCollection.AddTransient<DocumentController>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            _nodeController = serviceProvider.GetService<NodeController>();
            _feedController = serviceProvider.GetService<FeedController>();
            _documentController = serviceProvider.GetService<DocumentController>();
        }

        protected override void SetupPublicCommunityEntity(string entityId)
        {
            var node = new Data.Node { Id = ObjectId.Parse(entityId), Title = "A public hub", ContentPrivacy = PrivacyLevel.Public };
            SetupNode(node);
        }

        protected override void SetupEcosystemCommunityEntity(string entityId)
        {
            var node = new Data.Node { Id = ObjectId.Parse(entityId), Title = "An ecosystem hub", ContentPrivacy = PrivacyLevel.Ecosystem };
            SetupNode(node);
        }

        protected override void SetupPrivateCommunityEntity(string entityId)
        {
            var node = new Data.Node { Id = ObjectId.Parse(entityId), Title = "A private hub", ContentPrivacy = PrivacyLevel.Private };
            SetupNode(node);
        }

        protected override async Task<IActionResult> Get(string ceId)
        {
            return await _nodeController.Get(ceId);
        }

        protected override async Task<IActionResult> GetFeed(string ceId)
        {
            return await _feedController.GetFeedData(ceId, null, ContentEntityFilter.Posts, new SearchModel());
        }

        protected override async Task<IActionResult> GetDocuments(string ceId)
        {
            return await _nodeController.GetDocuments(ceId, null, null, new SearchModel());
        }

        private void SetupNode(Data.Node node)
        {
            SetupCommunityEntity(node.Id.ToString());
            _nodeRepository.Setup(s => s.Get(node.Id.ToString())).Returns(node);
            _nodeRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Data.Node> { node });
            _feedRepository.Setup(s => s.Get(node.Id.ToString())).Returns(new Feed { Id = ObjectId.Parse(node.Id.ToString()), Type = FeedType.Node });
        }

        private void SetupEcosystemProjectMember(string entityId)
        {
            string projectId = "projectId";
            string userId = "userId";

            SetupBaseContext();
            _contextService.Setup(s => s.CurrentUserId).Returns(userId);
            _relationRepository.Setup(s => s.ListForSourceOrTargetIds(It.IsAny<IEnumerable<string>>())).Returns(new List<Relation> { new Relation { SourceCommunityEntityId = projectId, SourceCommunityEntityType = CommunityEntityType.Project, TargetCommunityEntityId = entityId, TargetCommunityEntityType = CommunityEntityType.Workspace } });
            _membershipRepository.Setup(s => s.List(It.IsAny<Expression<Func<Membership, bool>>>())).Returns(new List<Membership> { new Membership { UserId = userId, CommunityEntityId = projectId, CommunityEntityType = CommunityEntityType.Project } });
        }

        private void SetupEcosystemCommunityMember(string entityId)
        {
            string communityId = "communityId";
            string userId = "userId";

            SetupBaseContext();
            _contextService.Setup(s => s.CurrentUserId).Returns(userId);
            _relationRepository.Setup(s => s.ListForSourceOrTargetIds(It.IsAny<IEnumerable<string>>())).Returns(new List<Relation> { new Relation { SourceCommunityEntityId = communityId, SourceCommunityEntityType = CommunityEntityType.Workspace, TargetCommunityEntityId = entityId, TargetCommunityEntityType = CommunityEntityType.Node } });
            _membershipRepository.Setup(s => s.List(It.IsAny<Expression<Func<Membership, bool>>>())).Returns(new List<Membership> { new Membership { UserId = userId, CommunityEntityId = communityId, CommunityEntityType = CommunityEntityType.Workspace } });
        }


        [Test]
        public async Task GetPublicCommunityEntityAsEcosystemProjectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemProjectMember(ceId);
            SetupPublicCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetEcosystemCommunityEntityAsEcosystemProjectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemProjectMember(ceId);
            SetupEcosystemCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetPrivateCommunityEntityAsEcosystemProjectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemProjectMember(ceId);
            SetupPrivateCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }

        [Test]
        public async Task GetPublicCommunityEntityAsEcosystemCommunityMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemCommunityMember(ceId);
            SetupPublicCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetEcosystemCommunityEntityAsEcosystemCommunityMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemCommunityMember(ceId);
            SetupEcosystemCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetPrivateCommunityEntityAsEcosystemCommunityMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupEcosystemCommunityMember(ceId);
            SetupPrivateCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }
    }
}