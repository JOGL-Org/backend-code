using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "users";
        protected override Expression<Func<User, string>> AutocompleteField => u => u.FullName;
        protected override Expression<Func<User, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<User, object>>[] { u => u.FirstName, u => u.LastName, u => u.Username };
            }
        }

        public override Expression<Func<User, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.CreatedUTC;
                case SortKey.Alphabetical:
                    return (e) => e.FullName;
                default:
                    return null;
            }
        }

        public User GetForEmail(string email)
        {
            var coll = GetCollection<User>();
            return coll.Find(e => e.Email == email && !e.Deleted, new FindOptions { Collation = DefaultCollation }).FirstOrDefault();
        }

        public async Task SetPasswordAsync(string userId, string passwordHash, byte[] salt)
        {
            await UpdateAsync(userId, Builders<User>.Update.Set(e => e.PasswordHash, passwordHash)
                                                           .Set(e => e.PasswordSalt, salt)
                                                           .Set(e => e.UpdatedUTC, DateTime.UtcNow)
                                                           .Set(e => e.UpdatedByUserId, userId));
        }

        public async Task SetVerifiedAsync(string userId)
        {
            await UpdateAsync(userId, Builders<User>.Update.Set(e => e.Status, UserStatus.Verified)
                                                           .Set(e => e.UpdatedUTC, DateTime.UtcNow)
                                                           .Set(e => e.UpdatedByUserId, userId));
        }

        protected override UpdateDefinition<User> GetDefaultUpdateDefinition(User updatedEntity)
        {
            var fullname = updatedEntity.FullName;
            return Builders<User>.Update.Set(e => e.FirstName, updatedEntity.FirstName)
                                        .Set(e => e.LastName, updatedEntity.LastName)
                                        .Set(e => e.FullName, updatedEntity.FullName)
                                        .Set(e => e.Username, updatedEntity.Username)
                                        .Set(e => e.DateOfBirth, updatedEntity.DateOfBirth)
                                        .Set(e => e.Email, updatedEntity.Email)
                                        .Set(e => e.BannerId, updatedEntity.BannerId)
                                        .Set(e => e.AvatarId, updatedEntity.AvatarId)
                                        .Set(e => e.Bio, updatedEntity.Bio)
                                        .Set(e => e.ShortBio, updatedEntity.ShortBio)
                                        .Set(e => e.StatusText, updatedEntity.StatusText)
                                        .Set(e => e.Gender, updatedEntity.Gender)
                                        .Set(e => e.Country, updatedEntity.Country)
                                        .Set(e => e.City, updatedEntity.City)
                                        .Set(e => e.Assets, updatedEntity.Assets)
                                        .Set(e => e.OrcidId, updatedEntity.OrcidId)
                                        .Set(e => e.Skills, updatedEntity.Skills)
                                        .Set(e => e.Interests, updatedEntity.Interests)
                                        .Set(e => e.ContactMe, updatedEntity.ContactMe)
                                        .Set(e => e.Newsletter, updatedEntity.Newsletter)
                                        .Set(e => e.Experience, updatedEntity.Experience)
                                        .Set(e => e.Education, updatedEntity.Education)
                                        .Set(e => e.Auth, updatedEntity.Auth)
                                        .Set(e => e.NotificationsReadUTC, updatedEntity.NotificationsReadUTC)
                                        .Set(e => e.Language, updatedEntity.Language)
                                        .Set(e => e.Links, updatedEntity.Links)
                                        .Set(e => e.NotificationSettings, updatedEntity.NotificationSettings)
                                        .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                        .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public async override Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<User>();
            await coll.Indexes.CreateOneAsync(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username), new CreateIndexOptions { Unique = true }));
            await coll.Indexes.CreateOneAsync(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true }));

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));

            if (!searchIndexes.Contains(INDEX_AUTOCOMPLETE))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_AUTOCOMPLETE, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", false }, { "fields", new BsonDocument { { nameof(User.FullName), new BsonDocument { { "tokenization", "nGram" }, { "type", "autocomplete" } } } } } } } })));
        }
    }
}