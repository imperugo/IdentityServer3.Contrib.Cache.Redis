# IdentityServer3.Contrib.Cache.Redis

IdentityServer3.Contrib.Cache.Redis is a library that offers the right implementation of 

- ```ICache<Client>```
- ```ICache<IEnumerable<Scope>>```
- ```ICache<IEnumerable<Claim>>```

for all distributed environments that are using Redis as cache server.

##How to install it

##Install Core [![NuGet Status](http://img.shields.io/nuget/v/IdentityServer3.Contrib.Cache.Redis.svg?style=flat)](http://www.nuget.org/packages/IdentityServer3.Contrib.Cache.Redis/)

```
PM> Install-Package IdentityServer3.Contrib.Cache.Redis
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


## Contributing
**Getting started with Git and GitHub**

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
 * [Open an issue](https://github.com/imperugo/StackExchange.Redis.Extensions/issues) if you encounter a bug or have a suggestion for improvements/features


Once you're familiar with Git and GitHub, clone the repository and start contributing.
