using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer3.Contrib.Cache.Redis
{
    public class RedisRefreshTokenStore : IRefreshTokenStore
    {
        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public Task<RefreshToken> GetAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string subject, string client)
        {
            throw new NotImplementedException();
        }

        public Task StoreAsync(string key, RefreshToken value)
        {
            throw new NotImplementedException();
        }
    }
}
