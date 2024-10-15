using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ImageRepository : BaseRepository<Image>, IImageRepository
    {
        public ImageRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "images";

        protected override UpdateDefinition<Image> GetDefaultUpdateDefinition(Image updatedEntity)
        {
            return Builders<Image>.Update.Set(e => e.Name, updatedEntity.Name)
                                         .Set(e => e.Filename, updatedEntity.Filename)
                                         .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                         .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}