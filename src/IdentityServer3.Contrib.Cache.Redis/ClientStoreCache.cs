using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3.Contrib.Cache.Redis
{
	public class ClientStoreCache : ICache<Client>
	{
		private readonly ICacheClient cacheClient;

		public ClientStoreCache(ConnectionMultiplexer connection)
			: this(new StackExchangeRedisCacheClient(connection, new NewtonsoftSerializer()))
		{
		}

		public ClientStoreCache(ICacheClient cacheClient)
		{
			this.cacheClient = cacheClient;
		}

		public Task<Client> GetAsync(string key)
		{
			return cacheClient.GetAsync<Client>(key);
		}

		public Task SetAsync(string key, Client item)
		{
			return cacheClient.AddAsync(key, item, TimeSpan.FromHours(2));
		}
	}
}