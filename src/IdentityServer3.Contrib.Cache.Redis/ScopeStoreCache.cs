using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Contrib.Cache.Redis.CacheClient;

namespace IdentityServer3.Contrib.Cache.Redis
{
    public class ScopeStoreCache : ICache<IEnumerable<Scope>>
	{
		private readonly ICacheManager cacheClient;

		public ScopeStoreCache(ConnectionMultiplexer connection)
			: this(new RedisCacheManager(connection))
		{
		}

		public ScopeStoreCache(ICacheManager cacheClient)
		{
			this.cacheClient = cacheClient;
		}

		public Task<IEnumerable<Scope>> GetAsync(string key)
		{
			return cacheClient.GetAsync<IEnumerable<Scope>>(key);
		}

		public Task SetAsync(string key, IEnumerable<Scope> item)
		{
			return cacheClient.AddAsync(key, item, TimeSpan.FromHours(2));
		}
	}
}