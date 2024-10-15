using Jogl.Server.API.Controllers;
using Jogl.Server.API.Model;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Moq;

namespace Jogl.Server.API.Test
{
    public class CallForProposalControllerTests : CommunityEntityControllerTestBase<CallForProposalModel>
    {
        private CallForProposalController _callForProposalController;
        private FeedController _feedController;
        private DocumentController _documentController;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _serviceCollection.AddTransient<CallForProposalController>();
            _serviceCollection.AddTransient<FeedController>();
            _serviceCollection.AddTransient<DocumentController>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            _callForProposalController = serviceProvider.GetService<CallForProposalController>();
            _feedController = serviceProvider.GetService<FeedController>();
            _documentController = serviceProvider.GetService<DocumentController>();
        }

        protected override void SetupPublicCommunityEntity(string entityId)
        {
            var cfp = new CallForProposal { Id = ObjectId.Parse(entityId), Title = "A public cfp", ContentPrivacy = PrivacyLevel.Public };
            SetupCallForProposal(cfp);
        }

        protected override void SetupEcosystemCommunityEntity(string entityId)
        {
            var cfp = new CallForProposal { Id = ObjectId.Parse(entityId), Title = "An ecosystem cfp", ContentPrivacy = PrivacyLevel.Ecosystem };
            SetupCallForProposal(cfp);
        }

        protected override void SetupPrivateCommunityEntity(string entityId)
        {
            var cfp = new CallForProposal { Id = ObjectId.Parse(entityId), Title = "A private cfp", ContentPrivacy = PrivacyLevel.Private };
            SetupCallForProposal(cfp);
        }

        protected override async Task<IActionResult> Get(string ceId)
        {
            return await _callForProposalController.Get(ceId);
        }

        protected override async Task<IActionResult> GetFeed(string ceId)
        {
            return await _feedController.GetFeedData(ceId, null, ContentEntityFilter.Posts, new SearchModel());
        }

        protected override async Task<IActionResult> GetDocuments(string ceId)
        {
            return await _callForProposalController.GetDocuments(ceId, null, null, new SearchModel());
        }

        private void SetupCallForProposal(CallForProposal callForProposal)
        {
            var community = new Workspace { Id = ObjectId.GenerateNewId(), Title = "Parent community" };
            callForProposal.ParentCommunityEntityId = community.Id.ToString();

            SetupCommunityEntity(callForProposal.Id.ToString());

            _workspaceRepository.Setup(s => s.Get(community.Id.ToString())).Returns(community);
            _workspaceRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Workspace> { community });

            _callForProposalsRepository.Setup(s => s.Get(callForProposal.Id.ToString())).Returns(callForProposal);
            _callForProposalsRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<CallForProposal> { callForProposal });
            _feedRepository.Setup(s => s.Get(callForProposal.Id.ToString())).Returns(new Feed { Id = ObjectId.Parse(callForProposal.Id.ToString()), Type = FeedType.CallForProposal });
        }
    }
}