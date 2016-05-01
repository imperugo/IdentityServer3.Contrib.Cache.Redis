using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace IdentityServer3.Contrib.Cache.RedisTests
{
	[TestClass]
	public class UserServiceCacheTests
	{
		[TestMethod]
		public void SetClaimCache()
		{
			var redisConnString = "localhost:6379";
			var options = ConfigurationOptions.Parse(redisConnString);
			options.AllowAdmin = true;
			options.Proxy = Proxy.Twemproxy;

			var connection = ConnectionMultiplexer.Connect(options);
			ICacheManager cacheClient = new RedisCacheManager(connection);

			var claim = new Claim("test_claim", "testClaimValue");

			cacheClient.Add("testClaimCache", claim, TimeSpan.FromMinutes(30));
			var claimFromCache = cacheClient.Get<Claim>("testClaimCache");
			Assert.IsNotNull(claimFromCache);

			IEnumerable<Claim> claims = new List<Claim> {claim, claim, claim};

			cacheClient.Add("testClaimCacheList", claims, TimeSpan.FromMinutes(30));
			var claimsFromCache = cacheClient.Get<IEnumerable<Claim>>("testClaimCacheList");
			Assert.IsNotNull(claimsFromCache);
		}
	}
}