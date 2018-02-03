using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using StackExchange.Redis;

namespace IdentityServer3.Contrib.Cache.Redis
{
    public class RedisAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private readonly ICacheManager cacheClient;

        public RedisAuthorizationCodeStore(ConnectionMultiplexer connection)
            : this(new RedisCacheManager(connection)) { }

        public RedisAuthorizationCodeStore(ICacheManager cacheClient)
            => this.cacheClient = cacheClient;

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
            => throw new System.NotImplementedException();

        public Task<AuthorizationCode> GetAsync(string key)
            => cacheClient.GetAsync<AuthorizationCode>(key);

        public Task RemoveAsync(string key)
            => cacheClient.RemoveAsync(key);

        public Task RevokeAsync(string subject, string client)
            => throw new System.NotImplementedException();

        public Task StoreAsync(string key, AuthorizationCode value)
            => cacheClient.AddAsync(key, value);
    }
}
