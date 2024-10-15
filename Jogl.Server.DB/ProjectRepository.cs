using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ProjectRepository : CommunityEntityRepository<Project>, IProjectRepository
    {
        public ProjectRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "projects";

        protected override UpdateDefinition<Project> GetDefaultUpdateDefinition(Project updatedEntity)
        {
            return Builders<Project>.Update.Set(e => e.Title, updatedEntity.Title)
                                           .Set(e => e.ShortTitle, updatedEntity.ShortTitle)
                                           .Set(e => e.Description, updatedEntity.Description)
                                           .Set(e => e.ShortDescription, updatedEntity.ShortDescription)
                                           .Set(e => e.BannerId, updatedEntity.BannerId)
                                           .Set(e => e.LogoId, updatedEntity.LogoId)
                                           .Set(e => e.Interests, updatedEntity.Interests)
                                           .Set(e => e.Keywords, updatedEntity.Keywords)
                                           .Set(e => e.Links, updatedEntity.Links)
                                           .Set(e => e.Onboarding, updatedEntity.Onboarding)
                                           .Set(e => e.Status, updatedEntity.Status)
                                           .Set(e => e.ListingPrivacy, updatedEntity.ListingPrivacy)
                                           .Set(e => e.ContentPrivacy, updatedEntity.ContentPrivacy)
                                           .Set(e => e.ContentPrivacyCustomSettings, updatedEntity.ContentPrivacyCustomSettings)
                                           .Set(e => e.JoiningRestrictionLevel, updatedEntity.JoiningRestrictionLevel)
                                           .Set(e => e.JoiningRestrictionLevelCustomSettings, updatedEntity.JoiningRestrictionLevelCustomSettings)
                                           .Set(e => e.Tabs, updatedEntity.Tabs)
                                           .Set(e => e.Settings, updatedEntity.Settings)
                                           .Set(e => e.HomeChannelId, updatedEntity.HomeChannelId)
                                           .Set(e => e.Maturity, updatedEntity.Maturity)
                                           .Set(e => e.IsLookingForCollaborators, updatedEntity.IsLookingForCollaborators)
                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}