using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using IdentityServer3.Core.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.Contrib.Cache.Redis
{
    public class UserServiceCache : ICache<IEnumerable<Claim>>
	{
		private readonly ICacheManager cacheClient;

		public UserServiceCache(ConnectionMultiplexer connection)
			: this(new RedisCacheManager(connection))
		{
		}

		public UserServiceCache(ICacheManager cacheClient)
		{
			this.cacheClient = cacheClient;
		}

		public Task<IEnumerable<Claim>> GetAsync(string key)
		{
			return cacheClient.GetAsync<IEnumerable<Claim>>(key);
		}

		public Task SetAsync(string key, IEnumerable<Claim> item)
		{
			return cacheClient.AddAsync(key, item, TimeSpan.FromMinutes(30));
		}
	}
}