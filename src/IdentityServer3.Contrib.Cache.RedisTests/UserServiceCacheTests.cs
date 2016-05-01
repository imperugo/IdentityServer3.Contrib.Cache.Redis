using Microsoft.VisualStudio.TestTools.UnitTesting;
using IdentityServer3.Contrib.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Contrib.Cache.Redis.CacheClient;
using StackExchange.Redis;
using System.Security.Claims;

namespace IdentityServer3.Contrib.Cache.Redis.Tests
{
    [TestClass()]
    public class UserServiceCacheTests
    {
        [TestMethod()]
        public void SetClaimCache()
        {

            string redisConnString = "10.1.9.63:22121";
            ConfigurationOptions options = ConfigurationOptions.Parse(redisConnString);
            options.AllowAdmin = true;
            options.Proxy = Proxy.Twemproxy;

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);
            ICacheManager cacheClient = new RedisCacheManager(connection);

            Claim claim = new Claim("test_claim", "testClaimValue");

            cacheClient.Add("testClaimCache", claim, TimeSpan.FromMinutes(30));
            Claim claimFromCache = cacheClient.Get<Claim>("testClaimCache");
            Assert.IsNotNull(claimFromCache);

            IEnumerable<Claim> claims = new List<Claim> { claim, claim, claim };

            cacheClient.Add("testClaimCacheList", claims, TimeSpan.FromMinutes(30));
            IEnumerable<Claim> claimsFromCache = cacheClient.Get<IEnumerable<Claim>>("testClaimCacheList");
            Assert.IsNotNull(claimsFromCache);

        }
    }
}