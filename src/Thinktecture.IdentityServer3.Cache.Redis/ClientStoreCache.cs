using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer3.Cache.Redis
{
	public class ClientStoreCache : ICache<Client>
	{
		private readonly ICacheClient cacheClient;

		public ClientStoreCache(ConnectionMultiplexer connection)
			: this(new StackExchangeRedisCacheClient(connection, new JsonSerializer()))
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