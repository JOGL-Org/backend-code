using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class OrganizationRepository : CommunityEntityRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "organizations";

        protected override UpdateDefinition<Organization> GetDefaultUpdateDefinition(Organization updatedEntity)
        {
            return Builders<Organization>.Update.Set(e => e.Title, updatedEntity.Title)
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
                                                .Set(e => e.Address, updatedEntity.Address)
                                                .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}