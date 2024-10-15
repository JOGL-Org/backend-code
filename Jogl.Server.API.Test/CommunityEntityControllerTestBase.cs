using Jogl.Server.API.Model;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using System.Linq.Expressions;

namespace Jogl.Server.API.Test
{
    public abstract class CommunityEntityControllerTestBase<TModel> : ControllerTestBase
    {
        protected abstract Task<IActionResult> Get(string ceId);
        protected abstract Task<IActionResult> GetFeed(string ceId);
        protected abstract Task<IActionResult> GetDocuments(string ceId);

        protected void SetupBaseContext()
        {
            _relationRepository.Setup(s => s.List(It.IsAny<Expression<Func<Relation, bool>>>())).Returns(new List<Relation>());
            _relationRepository.Setup(s => s.ListForSourceOrTargetIds(It.IsAny<IEnumerable<string>>())).Returns(new List<Relation>());
            _needRepository.Setup(s => s.ListForEntityIds(It.IsAny<IEnumerable<string>>())).Returns(new List<Need>());
            _membershipRepository.Setup(s => s.List(It.IsAny<Expression<Func<Membership, bool>>>())).Returns(new List<Membership>());
            _membershipRepository.Setup(s => s.ListForCommunityEntities(It.IsAny<IEnumerable<string>>())).Returns(new List<Membership>());
            _invitationRepository.Setup(s => s.List(It.IsAny<Expression<Func<Invitation, bool>>>())).Returns(new List<Invitation>());
            _membershipRepository.Setup(s => s.List(It.IsAny<Expression<Func<Membership, bool>>>())).Returns(new List<Membership>());
            _eventAttendanceRepository.Setup(s => s.List(It.IsAny<Expression<Func<EventAttendance, bool>>>())).Returns(new List<EventAttendance> { });
        }

        protected void SetupCommunityEntity(string entityId)
        {
            _workspaceRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Workspace> { });
            _nodeRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Data.Node> { });
            _organizationRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Organization> { });
            _callForProposalsRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<CallForProposal> { });
            _callForProposalsRepository.Setup(s => s.List(It.IsAny<Expression<Func<CallForProposal, bool>>>())).Returns(new List<CallForProposal>());
            _eventRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Event> { });
            _needRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Need> { });
            _proposalRepository.Setup(s => s.List(It.IsAny<Expression<Func<Proposal, bool>>>())).Returns(new List<Proposal>());
            _paperRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Paper> { });
            _userRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<User> { });
            _contentEntityRepository.Setup(s => s.List(It.IsAny<IEnumerable<string>>())).Returns(new List<ContentEntity>());
            _contentEntityRepository.Setup(s => s.List(It.IsAny<Expression<Func<ContentEntity, bool>>>())).Returns(new List<ContentEntity>());
            _commentRepository.Setup(s => s.List(It.IsAny<Expression<Func<Comment, bool>>>())).Returns(new List<Comment>());
            _mentionRepository.Setup(s => s.List(It.IsAny<Expression<Func<Mention, bool>>>())).Returns(new List<Mention> { });
            _documentRepository.Setup(s => s.List(It.IsAny<Expression<Func<Document, bool>>>())).Returns(new List<Document> {
                new Document { Id = ObjectId.GenerateNewId(), Name = "Document 1 - File", FeedId = entityId, Filename = "Testfile.txt", Type = DocumentType.Document },
                new Document { Id = ObjectId.GenerateNewId(), Name = "Document 2 - URL", FeedId = entityId, URL="https://hostname.com/Testfile.txt", Type = DocumentType.Link },
                new Document { Id = ObjectId.GenerateNewId(), Name = "Document 3 - JOGL Doc", FeedId = entityId, Description="THIS IS MY JOGL DOC", Type = DocumentType.JoglDoc }
            });
        }

        protected void SetupNonMember()
        {
            SetupBaseContext();
            _contextService.Setup(s => s.CurrentUserId).Returns(string.Empty);
        }

        protected void SetupInvitee(string userId, string entityId)
        {
            SetupBaseContext();
            _contextService.Setup(s => s.CurrentUserId).Returns(userId);
            _invitationRepository.Setup(s => s.List(It.IsAny<Expression<Func<Invitation, bool>>>())).Returns(new List<Invitation> { new Invitation { InviteeUserId = userId, CommunityEntityId = entityId, CommunityEntityType = CommunityEntityType.Project } });
        }

        protected void SetupDirectMember(string userId, string entityId)
        {
            SetupBaseContext();
            _contextService.Setup(s => s.CurrentUserId).Returns(userId);
            _membershipRepository.Setup(s => s.List(It.IsAny<Expression<Func<Membership, bool>>>())).Returns(new List<Membership> { new Membership { UserId = userId, CommunityEntityId = entityId, CommunityEntityType = CommunityEntityType.Project } });
        }

        protected abstract void SetupPublicCommunityEntity(string entityId);
        protected abstract void SetupEcosystemCommunityEntity(string entityId);
        protected abstract void SetupPrivateCommunityEntity(string entityId);

        protected async Task AssertEntityReadableAsync(string communityEntityId)
        {
            var result = await Get(communityEntityId);
            AssertIsReturned<TModel>(result);

            var feedResult = await GetFeed(communityEntityId);
            AssertIsReturned<DiscussionStatModel>(feedResult);

            var documentResult = await GetDocuments(communityEntityId);
            AssertIsReturned<IEnumerable<DocumentModel>>(documentResult);
        }

        protected async Task AssertEntityNotReadableAsync(string communityEntityId)
        {
            var result = await Get(communityEntityId);
            AssertIsReturned<CommunityEntityMiniModel>(result);

            var feedResult = await GetFeed(communityEntityId);
            AssertIsForbidden(feedResult);

            var documentResult = await GetDocuments(communityEntityId);
            AssertIsForbidden(documentResult);
        }

        [Test]
        public async Task GetPublicCommunityEntityAsNonMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupNonMember();
            SetupPublicCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetEcosystemCommunityEntityAsNonMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupNonMember();
            SetupEcosystemCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }

        [Test]
        public async Task GetPrivateCommunityEntityAsNonMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupNonMember();
            SetupPrivateCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }

        [Test]
        public async Task GetPublicCommunityEntityAsInvitee()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupInvitee("user_id", ceId);
            SetupPublicCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetEcosystemCommunityEntityAsInvitee()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupInvitee("user_id", ceId);
            SetupEcosystemCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }

        [Test]
        public async Task GetPrivateCommunityEntityAsInvitee()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupInvitee("user_id", ceId);
            SetupPrivateCommunityEntity(ceId);
            await AssertEntityNotReadableAsync(ceId);
        }

        [Test]
        public async Task GetPublicCommunityEntityAsDirectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupDirectMember("user_id", ceId);
            SetupPublicCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetEcosystemCommunityEntityAsDirectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupDirectMember("user_id", ceId);
            SetupEcosystemCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }

        [Test]
        public async Task GetPrivateCommunityEntityAsDirectMember()
        {
            var ceId = ObjectId.GenerateNewId().ToString();

            SetupDirectMember("user_id", ceId);
            SetupPrivateCommunityEntity(ceId);
            await AssertEntityReadableAsync(ceId);
        }
    }
}