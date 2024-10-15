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
    public class OrganizationControllerTests : CommunityEntityControllerTestBase<OrganizationModel>
    {
        private OrganizationController _organizationController;
        private FeedController _feedController;
        private DocumentController _documentController;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _serviceCollection.AddTransient<OrganizationController>();
            _serviceCollection.AddTransient<FeedController>();
            _serviceCollection.AddTransient<DocumentController>();

            var serviceProvider = _serviceCollection.BuildServiceProvider();
            _organizationController = serviceProvider.GetService<OrganizationController>();
            _feedController = serviceProvider.GetService<FeedController>();
            _documentController = serviceProvider.GetService<DocumentController>();
        }

        protected override void SetupPublicCommunityEntity(string entityId)
        {
            var organization = new Organization { Id = ObjectId.Parse(entityId), Title = "A public organization", ContentPrivacy = PrivacyLevel.Public };
            SetupOrganization(organization);
        }

        protected override void SetupEcosystemCommunityEntity(string entityId)
        {
            var organization = new Organization { Id = ObjectId.Parse(entityId), Title = "An ecosystem organization", ContentPrivacy = PrivacyLevel.Ecosystem };
            SetupOrganization(organization);
        }

        protected override void SetupPrivateCommunityEntity(string entityId)
        {
            var organization = new Organization { Id = ObjectId.Parse(entityId), Title = "A private organization", ContentPrivacy = PrivacyLevel.Private };
            SetupOrganization(organization);
        }

        protected override async Task<IActionResult> Get(string ceId)
        {
            return await _organizationController.Get(ceId);
        }

        protected override async Task<IActionResult> GetFeed(string ceId)
        {
            return await _feedController.GetFeedData(ceId, null, ContentEntityFilter.Posts, new SearchModel());
        }

        protected override async Task<IActionResult> GetDocuments(string ceId)
        {
            return await _organizationController.GetDocuments(ceId, null, null, new SearchModel());
        }

        private void SetupOrganization(Organization organization)
        {
            SetupCommunityEntity(organization.Id.ToString());
            _organizationRepository.Setup(s => s.Get(organization.Id.ToString())).Returns(organization);
            _organizationRepository.Setup(s => s.Get(It.IsAny<List<string>>())).Returns(new List<Organization> { organization });
            _feedRepository.Setup(s => s.Get(organization.Id.ToString())).Returns(new Feed { Id = ObjectId.Parse(organization.Id.ToString()), Type = FeedType.Organization });
        }
    }
}