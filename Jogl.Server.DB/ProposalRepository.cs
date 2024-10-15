using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ProposalRepository : BaseRepository<Proposal>, IProposalRepository
    {
        public ProposalRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "proposals";

        public List<Proposal> ListForCFPIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Proposal>();
            var filterBuilder = Builders<Proposal>.Filter;
            var filter = filterBuilder.In(e => e.CallForProposalId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Proposal> GetDefaultUpdateDefinition(Proposal updatedEntity)
        {
            return Builders<Proposal>.Update.Set(e => e.Status, updatedEntity.Status)
                                            .Set(e => e.Title, updatedEntity.Title)
                                            .Set(e => e.Description, updatedEntity.Description)
                                            .Set(e => e.Answers, updatedEntity.Answers)
                                            .Set(e => e.Score, updatedEntity.Score)
                                            .Set(e => e.Submitted, updatedEntity.Submitted)
                                            .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}