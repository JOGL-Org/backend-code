using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Security.AccessControl;

namespace Jogl.Server.DB
{
    public class RelationRepository : BaseRepository<Relation>, IRelationRepository
    {
        public RelationRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "relations";

        public List<Relation> ListForSourceIds(IEnumerable<string> ids, CommunityEntityType targetType)
        {
            var coll = GetCollection<Relation>();
            var filterBuilder = Builders<Relation>.Filter;
            var list = ids.ToList();
            var filter = filterBuilder.In(e => e.SourceCommunityEntityId, list) & filterBuilder.Eq(e => e.Deleted, false) & filterBuilder.Eq(e => e.TargetCommunityEntityType, targetType);
            return coll.Find(filter).ToList();
        }

        public List<Relation> ListForSourceIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Relation>();
            var filterBuilder = Builders<Relation>.Filter;
            var list = ids.ToList();
            var filter = filterBuilder.In(e => e.SourceCommunityEntityId, list) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<Relation> ListForSourceOrTargetIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Relation>();
            var filterBuilder = Builders<Relation>.Filter;
            var list = ids.ToList();
            var filter = (filterBuilder.In(e => e.SourceCommunityEntityId, list) | filterBuilder.In(e => e.TargetCommunityEntityId, list)) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<Relation> ListForTargetIds(IEnumerable<string> ids, CommunityEntityType sourceType)
        {
            var coll = GetCollection<Relation>();
            var filterBuilder = Builders<Relation>.Filter;
            var list = ids.ToList();
            var filter = filterBuilder.In(e => e.TargetCommunityEntityId, list) & filterBuilder.Eq(e => e.Deleted, false) & filterBuilder.Eq(e => e.SourceCommunityEntityType, sourceType);
            return coll.Find(filter).ToList();
        }

        public List<Relation> ListForTargetIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Relation>();
            var filterBuilder = Builders<Relation>.Filter;
            var list = ids.ToList();
            var filter = filterBuilder.In(e => e.TargetCommunityEntityId, list) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Relation> GetDefaultUpdateDefinition(Relation updatedEntity)
        {
            return Builders<Relation>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                            .Set(e => e.SourceCommunityEntityType, updatedEntity.SourceCommunityEntityType) //TODO remove after migration
                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                            .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}