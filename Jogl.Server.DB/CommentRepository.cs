using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "comments";

        public List<Comment> ListForContentEntities(IEnumerable<string> contentEntityIds)
        {
            var coll = GetCollection<Comment>();
            var filterBuilder = Builders<Comment>.Filter;
            var list = contentEntityIds.ToList();
            var filter = filterBuilder.In(e => e.ContentEntityId, list) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Comment> GetDefaultUpdateDefinition(Comment updatedEntity)
        {
            return Builders<Comment>.Update.Set(e => e.Text, updatedEntity.Text)
                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}