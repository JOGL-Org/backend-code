using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Auth
{
    public class AuthChallengeService : IAuthChallengeService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        public AuthChallengeService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
        }
        
        public string GetChallenge(string key)
        {
            if (!_memoryCache.TryGetValue(key, out string cacheValue))
            {
                cacheValue = $"Verify your account with JOGL{Environment.NewLine}One-time token: {Guid.NewGuid()}";
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _memoryCache.Set(key, cacheValue, cacheEntryOptions);
            }

            return cacheValue;
        }
    }
}