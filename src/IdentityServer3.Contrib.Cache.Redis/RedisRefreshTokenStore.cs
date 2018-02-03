using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.Contrib.Cache.Redis
{
    public class RedisRefreshTokenStore : IRefreshTokenStore
    {
        private readonly ICacheManager cacheClient;

        public RedisRefreshTokenStore(ConnectionMultiplexer connection)
            : this(new RedisCacheManager(connection)) { }

        public RedisRefreshTokenStore(ICacheManager cacheClient)
            => this.cacheClient = cacheClient;

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var keys = await cacheClient.SearchKeysAsync($"*:{subject}:*");
            var list = await cacheClient.GetAllAsync<RefreshToken>(keys);

            return list.Cast<ITokenMetadata>();
        }

        public async Task<RefreshToken> GetAsync(string key)
        {
            var keys = (await cacheClient.SearchKeysAsync($"*:{key}"));
            if (!keys.Any()) return null;

            return await cacheClient.GetAsync<RefreshToken>(keys.First());
        }

        public async Task RemoveAsync(string key)
        {
            var keys = (await cacheClient.SearchKeysAsync($"*:{key}"));
            if (keys.Any())
                await cacheClient.RemoveAsync(keys.First());
        }

        public async Task RevokeAsync(string subject, string client)
        {
            var keys = await cacheClient.SearchKeysAsync($"{client}:{subject}:*");
            await cacheClient.RemoveAllAsync(keys);
        }

        public Task StoreAsync(string key, RefreshToken value)
            => cacheClient.AddAsync(GetRefreshTokenKey(key, value), value);

        private string GetRefreshTokenKey(string key, RefreshToken value)
            => $"{value.ClientId}:{value.SubjectId}:{key}";
    }
}
