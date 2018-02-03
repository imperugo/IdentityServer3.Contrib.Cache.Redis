using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
            => throw new NotImplementedException();

        public Task<RefreshToken> GetAsync(string key)
            => cacheClient.GetAsync<RefreshToken>(key);

        public Task RemoveAsync(string key)
            => cacheClient.RemoveAsync(key);

        public Task RevokeAsync(string subject, string client)
            => throw new NotImplementedException();

        public Task StoreAsync(string key, RefreshToken value)
            => cacheClient.AddAsync(key, value);
    }
}
