using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "events";

        protected override Expression<Func<Event, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Event, object>>[] { e => e.Title, e => e.Description, e => e.Keywords };
            }
        }

        protected override Expression<Func<Event, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.Start;
                case SortKey.Alphabetical:
                    return (e) => e.Title;
                default:
                    return null;
            }
        }

        protected override UpdateDefinition<Event> GetDefaultUpdateDefinition(Event updatedEntity)
        {
            return Builders<Event>.Update.Set(e => e.Title, updatedEntity.Title)
                                         .Set(e => e.Description, updatedEntity.Description)
                                         .Set(e => e.GenerateMeetLink, updatedEntity.GenerateMeetLink)
                                         .Set(e => e.GenerateZoomLink, updatedEntity.GenerateZoomLink)
                                         .Set(e => e.MeetingURL, updatedEntity.MeetingURL)
                                         .Set(e => e.GeneratedMeetingURL, updatedEntity.GeneratedMeetingURL)
                                         .Set(e => e.Start, updatedEntity.Start)
                                         .Set(e => e.End, updatedEntity.End)
                                         .Set(e => e.BannerId, updatedEntity.BannerId)
                                         .Set(e => e.Timezone, updatedEntity.Timezone)
                                         .Set(e => e.Keywords, updatedEntity.Keywords)
                                         .Set(e => e.Visibility, updatedEntity.Visibility)
                                         .Set(e => e.Location, updatedEntity.Location)
                                         .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                         .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            var coll = GetCollection<Event>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}