using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class EventAttendanceRepository : BaseRepository<EventAttendance>, IEventAttendanceRepository
    {
        public EventAttendanceRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "eventAttendance";

        public async Task UpdateUserAsync(EventAttendance attendance)
        {
            var updateDefinition = Builders<EventAttendance>.Update
                                                  .Set(e => e.UserId, attendance.UserId)
                                                  .Set(e => e.UserEmail, attendance.UserEmail)
                                                  .Set(e => e.UpdatedUTC, attendance.UpdatedUTC)
                                                  .Set(e => e.UpdatedByUserId, attendance.UpdatedByUserId);

            await UpdateAsync(attendance.Id.ToString(), updateDefinition);
        }

        protected override UpdateDefinition<EventAttendance> GetDefaultUpdateDefinition(EventAttendance updatedEntity)
        {
            return Builders<EventAttendance>.Update.Set(e => e.Status, updatedEntity.Status)
                                                   .Set(e => e.AccessLevel, updatedEntity.AccessLevel)
                                                   .Set(e => e.Labels, updatedEntity.Labels)
                                                   .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                   .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }

        //public override async Task InitializeAsync()
        //{
        //    var coll = GetCollection<EventAttendance>();
            
        //    var searchIndexes = await ListIndexesAsync();
        //    if (!searchIndexes.Contains(INDEX_UNIQUE))
        //    {
        //        var builder = new IndexKeysDefinitionBuilder<EventAttendance>();
        //        var definition = builder.Ascending(u => u.EventId).Ascending(u => u.UserId).Ascending(u => u.UserEmail).Ascending(u => u.CommunityEntityId);
        //        await coll.Indexes.CreateOneAsync(new CreateIndexModel<EventAttendance>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE, Collation = new Collation("simple", strength: CollationStrength.Secondary) }));
        //    }
        //}
    }
}