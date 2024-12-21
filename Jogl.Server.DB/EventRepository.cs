using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
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

        public override Expression<Func<Event, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Alphabetical:
                    return e => e.Title;
                case SortKey.Date:
                    return e => e.Start;
                case SortKey.InvitationDate:
                    return e => e.UserInvitedUTC;
                default:
                    return base.GetSort(key);
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
            await EnsureExistsAsync();
            var coll = GetCollection<Event>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }

        public IFluentQuery<Event> QueryWithAttendanceData(string searchValue, string currentUserId)
        {
            var q = GetQuery(searchValue);

            var attendanceCollection = GetCollection<EventAttendance>("eventAttendance");

            q = q.Lookup<Event, EventAttendance, EventAttendance, List<EventAttendance>, Event>(
                foreignCollection: attendanceCollection,
                let: new BsonDocument("eventId", new BsonDocument("$toString", "$_id")),
                lookupPipeline: new EmptyPipelineDefinition<EventAttendance>()
                    .Match(new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { $"${nameof(EventAttendance.EventId)}", "$$eventId" }),
                        new BsonDocument("$eq", new BsonArray { $"${nameof(EventAttendance.UserId)}", $"{currentUserId}" })
                    }))),
                     @as: e => e.Attendances)
                .AppendStage<Event>(new BsonDocument("$addFields", new BsonDocument
                                {
                                    {
                                        nameof(Event.UserInvitedUTC), new BsonDocument("$cond", new BsonDocument
                                        {
                                            { "if", new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", $"$Attendances"), 0 }) },
                                            { "then", new BsonDocument("$arrayElemAt", new BsonArray { $"$Attendances.CreatedUTC", 0 }) },
                                            { "else", BsonNull.Value }
                                        })
                                    }
                                }));

            return new FluentQuery<Event>(_configuration, this, _context, q);
        }
    }
}