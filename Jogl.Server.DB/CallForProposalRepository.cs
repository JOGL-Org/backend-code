using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class CallForProposalRepository : CommunityEntityRepository<CallForProposal>, ICallForProposalRepository
    {
        public CallForProposalRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "callForProposals";

        public List<CallForProposal> ListForCommunityIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<CallForProposal>();
            var filterBuilder = Builders<CallForProposal>.Filter;
            var filter = filterBuilder.In(e => e.ParentCommunityEntityId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public async Task UpdateTemplateAsync(CallForProposal updatedEntity)
        {
            await UpdateAsync(updatedEntity.Id.ToString(), Builders<CallForProposal>.Update.Set(e => e.Template, updatedEntity.Template)
                                                                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC));
        }

        protected override UpdateDefinition<CallForProposal> GetDefaultUpdateDefinition(CallForProposal updatedEntity)
        {
            return Builders<CallForProposal>.Update.Set(e => e.Title, updatedEntity.Title)
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
                                                   //.Set(e => e.ParentCommunityEntityId, updatedEntity.ParentCommunityEntityId)
                                                   .Set(e => e.ProposalPrivacy, updatedEntity.ProposalPrivacy)
                                                   .Set(e => e.DiscussionParticipation, updatedEntity.DiscussionParticipation)
                                                   .Set(e => e.ProposalParticipation, updatedEntity.ProposalParticipation)
                                                   .Set(e => e.Template, updatedEntity.Template)
                                                   .Set(e => e.Scoring, updatedEntity.Scoring)
                                                   .Set(e => e.MaximumScore, updatedEntity.MaximumScore)
                                                   .Set(e => e.SubmissionsFrom, updatedEntity.SubmissionsFrom)
                                                   .Set(e => e.SubmissionsTo, updatedEntity.SubmissionsTo)
                                                   .Set(e => e.Rules, updatedEntity.Rules)
                                                   .Set(e => e.FAQ, updatedEntity.FAQ)
                                                   .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                   .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}