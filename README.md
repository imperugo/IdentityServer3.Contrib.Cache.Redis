# Thinktecture.IdentityServer3.Cache.Redis

Thinktecture.IdentityServer3.Cache.Redis is a library that offers the right implementation of 

- ```ICache<Client>```
- ```ICache<IEnumerable<Scope>>```
- ```ICache<IEnumerable<Claim>>```

for all distributed environments that are using Redis as cache server.

##How to install it

##Install Core [![NuGet Status](http://img.shields.io/nuget/v/Thinktecture.IdentityServer3.Cache.Redis.svg?style=flat)](https://www.nuget.org/Thinktecture.IdentityServer3.Cache.Redis/)

```
PM> Install-Package Thinktecture.IdentityServer3.Cache.Redis
```

Than register it on your Identity Server

```csharp
var clientStoreCache = new ClientStoreCache(myRedisMultiplexInstance);
var scopeStoreCache = new ScopeStoreCache(myRedisMultiplexInstance);
var userServiceCache = new UserServiceCache(myRedisMultiplexInstance);

factory.ConfigureClientStoreCache(new Registration<ICache<Client>>(clientStoreCache));

factory.ConfigureScopeStoreCache(new Registration<ICache<IEnumerable<Scope>>>(scopeStoreCache));

factory.ConfigureUserServiceCache(new Registration<ICache<IEnumerable<Claim>>>(userServiceCache));
```

For more information about caching and Identity Server Take a look [here](http://identityserver.github.io/Documentation/docs/advanced/caching.html)