using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using IdentityServer3.Core.Services;
using IdentityServer3.Contrib.Cache.Redis.CacheClient;

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