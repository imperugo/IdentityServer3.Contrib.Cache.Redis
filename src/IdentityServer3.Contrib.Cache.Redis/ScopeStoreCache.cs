using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace IdentityServer3.Contrib.Cache.Redis
{
	public class ScopeStoreCache : ICache<IEnumerable<Scope>>
	{
		private readonly ICacheClient cacheClient;

		public ScopeStoreCache(ConnectionMultiplexer connection)
			: this(new StackExchangeRedisCacheClient(connection, new NewtonsoftSerializer()))
		{
		}

		public ScopeStoreCache(ICacheClient cacheClient)
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