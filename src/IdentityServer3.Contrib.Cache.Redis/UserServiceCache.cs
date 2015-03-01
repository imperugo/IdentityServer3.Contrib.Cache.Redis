using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using Thinktecture.IdentityServer.Core.Services;

namespace IdentityServer3.Contrib.Cache.Redis
{
	public class UserServiceCache : ICache<IEnumerable<Claim>>
	{
		private readonly ICacheClient cacheClient;

		public UserServiceCache(ConnectionMultiplexer connection)
			: this(new StackExchangeRedisCacheClient(connection, new JsonSerializer()))
		{
		}

		public UserServiceCache(ICacheClient cacheClient)
		{
			this.cacheClient = cacheClient;
		}

		public async Task<IEnumerable<Claim>> GetAsync(string key)
		{
			var result = await cacheClient.GetAsync<object>(key);

			if (result != null)
			{
				return await Task.FromResult(result as IEnumerable<Claim>);
			}

			return await Task.FromResult<IEnumerable<Claim>>(null);
		}

		public Task SetAsync(string key, IEnumerable<Claim> item)
		{
			return cacheClient.AddAsync(key, item, TimeSpan.FromMinutes(30));
		}
	}
}