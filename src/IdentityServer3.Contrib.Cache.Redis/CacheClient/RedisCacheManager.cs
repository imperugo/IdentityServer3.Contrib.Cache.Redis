using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace IdentityServer3.Contrib.Cache.Redis.CacheClient
{
	/// <summary>
	///     Class to handle redis cache operation
	/// </summary>
	public class RedisCacheManager : ICacheManager
	{
		//private static ConnectionMultiplexer m_Multiplexer;
		private static string m_KeyPrefix;
		private static ICacheClient m_cacheClient;
		private static readonly ConcurrentDictionary<string, object> lockerObject = new ConcurrentDictionary<string, object>();
		private static readonly bool isCachingEnabled = false;

		public RedisCacheManager()
		{
			m_KeyPrefix = ConfigurationManager.AppSettings["Cache:KeyPrefix"];
			ISerializer serializer;

			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new ClaimConverter());
			serializer = new NewtonsoftSerializer(settings);

			if (string.IsNullOrEmpty(m_KeyPrefix))
			{
				m_KeyPrefix = "idscache";
			}
			m_cacheClient = new StackExchangeRedisCacheClient(serializer);
		}

		public RedisCacheManager(ConnectionMultiplexer connection)
		{
			m_KeyPrefix = ConfigurationManager.AppSettings["Cache:KeyPrefix"];
			ISerializer serializer;

			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new ClaimConverter());
			serializer = new NewtonsoftSerializer(settings);

			if (string.IsNullOrEmpty(m_KeyPrefix))
			{
				m_KeyPrefix = "idscache";
			}
			m_cacheClient = new StackExchangeRedisCacheClient(connection, serializer);
		}

		/// <summary>
		///     Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     True if the key is present into Redis. Othwerwise False
		/// </returns>
		public bool Exists(string key)
		{
			return m_cacheClient.Exists(key);
		}

		/// <summary>
		///     Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     True if the key is present into Redis. Othwerwise False
		/// </returns>
		public Task<bool> ExistsAsync(string key)
		{
			return m_cacheClient.ExistsAsync(GenerateKey(key));
		}

		/// <summary>
		///     Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key has removed. Othwerwise False
		/// </returns>
		public bool Remove(string key)
		{
			return m_cacheClient.Remove(GenerateKey(key));
		}

		/// <summary>
		///     Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key has removed. Othwerwise False
		/// </returns>
		public Task<bool> RemoveAsync(string key)
		{
			return m_cacheClient.RemoveAsync(GenerateKey(key));
		}

		/// <summary>
		///     Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		public void RemoveAll(IEnumerable<string> keys)
		{
			var newKeys = keys.Select(key => GenerateKey(key)).AsEnumerable();

			m_cacheClient.RemoveAll(newKeys);
		}

		/// <summary>
		///     Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		/// <returns></returns>
		public Task RemoveAllAsync(IEnumerable<string> keys)
		{
			var newKeys = keys.Select(key => GenerateKey(key)).AsEnumerable();
			return m_cacheClient.RemoveAllAsync(newKeys);
		}

		/// <summary>
		///     Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     Null if not present, otherwise the instance of T.
		/// </returns>
		public T Get<T>(string key)
		{
			return m_cacheClient.Get<T>(GenerateKey(key));
		}

		/// <summary>
		///     Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     Null if not present, otherwise the instance of T.
		/// </returns>
		public async Task<T> GetAsync<T>(string key)
		{
			return await m_cacheClient.GetAsync<T>(GenerateKey(key));
		}

		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value)
		{
			return m_cacheClient.Add(GenerateKey(key), value);
		}

		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value)
		{
			return await m_cacheClient.AddAsync(GenerateKey(key), value);
		}

		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value, DateTimeOffset expiresAt)
		{
			return m_cacheClient.Add(GenerateKey(key), value, expiresAt);
		}

		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt)
		{
			return await m_cacheClient.AddAsync(GenerateKey(key), value, expiresAt);
		}


		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value, TimeSpan expiresIn)
		{
			return m_cacheClient.Add(GenerateKey(key), value, expiresIn);
		}

		/// <summary>
		///     Adds the specified instance to the Redis m_cacheClient.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn)
		{
			return await m_cacheClient.AddAsync(GenerateKey(key), value, expiresIn);
		}

		/// <summary>
		///     Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		///     Empty list if there are no results, otherwise the instance of T.
		///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
		{
			var newKeys = keys.Select(key => GenerateKey(key)).AsEnumerable();
			return m_cacheClient.GetAll<T>(newKeys);
		}

		/// <summary>
		///     Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		///     Empty list if there are no results, otherwise the instance of T.
		///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
		{
			var newKeys = keys.Select(key => GenerateKey(key)).AsEnumerable();
			return await m_cacheClient.GetAllAsync<T>(newKeys);
		}

		/// <summary>
		///     Adds all.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items">The items.</param>
		public bool AddAll<T>(IList<Tuple<string, T>> items)
		{
			IList<Tuple<string, T>> newItems =
				items.Select(item => new Tuple<string, T>(GenerateKey(item.Item1), item.Item2)).ToList();
			return m_cacheClient.AddAll(newItems);
		}

		/// <summary>
		///     Adds all asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items)
		{
			IList<Tuple<string, T>> newItems =
				items.Select(item => new Tuple<string, T>(GenerateKey(item.Item1), item.Item2)).ToList();
			return await m_cacheClient.AddAllAsync(newItems);
		}

		/// <summary>
		///     Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool SetAdd<T>(string key, T item) where T : class
		{
			return m_cacheClient.SetAdd(GenerateKey(key), item);
		}

		/// <summary>
		///     Run SADD command http://redis.io/commands/sadd"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public async Task<bool> SetAddAsync<T>(string key, T item) where T : class
		{
			return await m_cacheClient.SetAddAsync(GenerateKey(key), item);
		}

		/// <summary>
		///     Run SMEMBERS command http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public string[] SetMember(string memberName)
		{
			return m_cacheClient.SetMember(GenerateKey(memberName));
		}

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns></returns>
		public async Task<string[]> SetMemberAsync(string memberName)
		{
			return await m_cacheClient.SetMemberAsync(GenerateKey(memberName));
		}

		///// <summary>
		/////     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		/////     Deserializes the results to T
		///// </summary>
		///// <typeparam name="T">The type of the expected objects</typeparam>
		///// <param name="key">The key</param>
		///// <returns>An array of objects in the set</returns>
		//public async Task<IEnumerable<T>> SetMembersAsync<T>(string key)
		//{
		//    return await m_cacheClient.SetMembersAsync<T>(GenerateKey(key));

		//}

		/// <summary>
		///     Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		///     Consider this as a command that should only be used in production environments with extreme care. It may ruin
		///     performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///     if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///     if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///     if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		public IEnumerable<string> SearchKeys(string pattern)
		{
			return m_cacheClient.SearchKeys(pattern);
		}

		/// <summary>
		///     Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		///     Consider this as a command that should only be used in production environments with extreme care. It may ruin
		///     performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///     if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///     if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///     if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
		{
			return m_cacheClient.SearchKeysAsync(pattern);
		}

		/// <summary>
		///     Gets the information about redis.
		///     More info see http://redis.io/commands/INFO
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> GetInfo()
		{
			return m_cacheClient.GetInfo();
		}

		/// <summary>
		///     Gets the information about redis.
		///     More info see http://redis.io/commands/INFO
		/// </summary>
		/// <returns></returns>
		public async Task<Dictionary<string, string>> GetInfoAsync()
		{
			return await m_cacheClient.GetInfoAsync();
		}

		/// <summary>
		///     Publishes a message to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public long Publish<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
		{
			return m_cacheClient.Publish(channel, message, flags);
		}

		/// <summary>
		///     Publishes a message to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public async Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
		{
			return await m_cacheClient.PublishAsync(channel, message, flags);
		}

		/// <summary>
		///     Registers a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Subscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
		{
			m_cacheClient.Subscribe(channel, handler, flags);
		}

		/// <summary>
		///     Registers a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler,
			CommandFlags flags = CommandFlags.None)
		{
			await m_cacheClient.SubscribeAsync(channel, handler, flags);
		}

		/// <summary>
		///     Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Unsubscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
		{
			m_cacheClient.Unsubscribe(channel, handler, flags);
		}

		/// <summary>
		///     Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler,
			CommandFlags flags = CommandFlags.None)
		{
			await m_cacheClient.UnsubscribeAsync(channel, handler, flags);
		}

		/// <summary>
		///     Unregisters all callback handlers on a channel.
		/// </summary>
		/// <param name="flags"></param>
		public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
		{
			m_cacheClient.UnsubscribeAll(flags);
		}

		/// <summary>
		///     Unregisters all callback handlers on a channel.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		public async Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
		{
			await m_cacheClient.UnsubscribeAllAsync(flags);
		}

		/// <summary>
		///     Insert the specified value at the head of the list stored at key. If key does not exist, it is created as empty
		///     list before performing the push operations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		///     the length of the list after the push operations.
		/// </returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <exception cref="System.ArgumentNullException">item;item cannot be null.</exception>
		/// <remarks>
		///     http://redis.io/commands/lpush
		/// </remarks>
		public long ListAddToLeft<T>(string key, T item) where T : class
		{
			return m_cacheClient.ListAddToLeft(GenerateKey(key), item);
		}

		/// <summary>
		///     Lists the add to left asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <exception cref="System.ArgumentNullException">item;item cannot be null.</exception>
		public async Task<long> ListAddToLeftAsync<T>(string key, T item) where T : class
		{
			return await m_cacheClient.ListAddToLeftAsync(GenerateKey(key), item);
		}

		/// <summary>
		///     Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <remarks>
		///     http://redis.io/commands/rpop
		/// </remarks>
		public T ListGetFromRight<T>(string key) where T : class
		{
			return m_cacheClient.ListGetFromRight<T>(GenerateKey(key));
		}

		/// <summary>
		///     Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <remarks>
		///     http://redis.io/commands/rpop
		/// </remarks>
		public async Task<T> ListGetFromRightAsync<T>(string key) where T : class
		{
			return await m_cacheClient.ListGetFromRightAsync<T>(GenerateKey(key));
		}


		/// <summary>
		///     Removes the specified fields from the hash stored at key.
		///     Specified fields that do not exist within this hash are ignored.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     If key is deleted returns true.
		///     If key does not exist, it is treated as an empty hash and this command returns false.
		/// </returns>
		public bool HashDelete(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashDelete(hashKey, GenerateKey(key), commandFlags);
		}

		/// <summary>
		///     Removes the specified fields from the hash stored at key.
		///     Specified fields that do not exist within this hash are ignored.
		///     If key does not exist, it is treated as an empty hash and this command returns 0.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields to be removed.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Tthe number of fields that were removed from the hash, not including specified but non existing fields.</returns>
		public long HashDelete(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashDelete(hashKey, keys.Select(x => GenerateKey(x)).AsEnumerable(), commandFlags);
		}

		/// <summary>
		///     Returns if field is an existing field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Returns if field is an existing field in the hash stored at key.</returns>
		public bool HashExists(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashExists(hashKey, GenerateKey(key), commandFlags);
		}

		/// <summary>
		///     Returns the value associated with field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
		public T HashGet<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashGet<T>(hashKey, GenerateKey(key), commandFlags);
		}

		/// <summary>
		///     Returns the values associated with the specified fields in the hash stored at key.
		///     For every field that does not exist in the hash, a nil value is returned.
		///     Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a
		///     list of nil values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being requested.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
		public Dictionary<string, T> HashGet<T>(string hashKey, IEnumerable<string> keys,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashGet<T>(hashKey, keys.Select(x => GenerateKey(x)).AsEnumerable(), commandFlags);
		}

		/// <summary>
		///     Returns all fields and values of the hash stored at key. In the returned value, every field name is followed by its
		///     value, so the length of the reply is twice the size of the hash.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
		public Dictionary<string, T> HashGetAll<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashGetAll<T>(hashKey, commandFlags);
		}

		/// <summary>
		///     Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new key
		///     holding a hash is created.
		///     If field does not exist the value is set to 0 before the operation is performed.
		///     The range of values supported by HINCRBY is limited to 64 bit signed integers.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		public long HashIncerementBy(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashIncerementBy(hashKey, GenerateKey(key), value, commandFlags);
		}

		/// <summary>
		///     Increment the specified field of an hash stored at key, and representing a floating point number, by the specified
		///     increment.
		///     If the field does not exist, it is set to 0 before performing the operation.
		/// </summary>
		/// <remarks>
		///     <para>
		///         An error is returned if one of the following conditions occur:
		///         * The field contains a value of the wrong type (not a string).
		///         * The current field content or the specified increment are not parsable as a double precision floating point
		///         number.
		///     </para>
		///     <para>
		///         Time complexity: O(1)
		///     </para>
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		public double HashIncerementBy(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashIncerementBy(hashKey, GenerateKey(key), value, commandFlags);
		}

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		public IEnumerable<string> HashKeys(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashKeys(hashKey, commandFlags);
		}

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		public long HashLength(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashLength(hashKey, commandFlags);
		}

		/// <summary>
		///     Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field
		///     already exists in the hash, it is overwritten.
		/// </summary>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="nx">Behave like hsetnx - set only if not exists</param>
		/// <param name="value">The value to be inserted</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     <c>true</c> if field is a new field in the hash and value was set.
		///     <c>false</c> if field already exists in the hash and no operation was performed.
		/// </returns>
		public bool HashSet<T>(string hashKey, string key, T value, bool nx = false,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashSet(hashKey, GenerateKey(key), value, nx, commandFlags);
		}

		/// <summary>
		///     Sets the specified fields to their respective values in the hash stored at key. This command overwrites any
		///     existing fields in the hash. If key does not exist, a new key holding a hash is created.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being set.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="values"></param>
		/// <param name="commandFlags">Command execution flags</param>
		public void HashSet<T>(string hashKey, Dictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
		{
			m_cacheClient.HashSet(hashKey,
				values.Select(v => new {key = GenerateKey(v.Key), value = v.Value}).ToDictionary(c => c.key, c => c.value),
				commandFlags);
		}

		/// <summary>
		///     Returns all values in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
		public IEnumerable<T> HashValues<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashValues<T>(hashKey, commandFlags);
		}

		/// <summary>
		///     iterates fields of Hash types and their associated values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1) for every call. O(N) for a complete iteration, including enough command calls for the cursor
		///     to return back to 0.
		///     N is the number of elements inside the collection.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="pattern">GLOB search pattern</param>
		/// <param name="pageSize">Number of elements to retrieve from the redis server in the cursor</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns></returns>
		public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return m_cacheClient.HashScan<T>(hashKey, pattern, pageSize, commandFlags);
		}

		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     If key is deleted returns true.
		///     If key does not exist, it is treated as an empty hash and this command returns false.
		/// </returns>
		public async Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashDeleteAsync(hashKey, GenerateKey(key), commandFlags);
		}

		/// <summary>
		///     Removes the specified fields from the hash stored at key.
		///     Specified fields that do not exist within this hash are ignored.
		///     If key does not exist, it is treated as an empty hash and this command returns 0.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields to be removed.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys">Keys to retrieve from the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Tthe number of fields that were removed from the hash, not including specified but non existing fields.</returns>
		public async Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return
				await m_cacheClient.HashDeleteAsync(hashKey, keys.Select(key => GenerateKey(key)).AsEnumerable(), commandFlags);
		}

		/// <summary>
		///     Returns if field is an existing field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Returns if field is an existing field in the hash stored at key.</returns>
		public async Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashExistsAsync(hashKey, GenerateKey(key), commandFlags);
		}


		/// <summary>
		///     Returns the value associated with field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
		public async Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashGetAsync<T>(hashKey, GenerateKey(key), commandFlags);
		}

		/// <summary>
		///     Returns the values associated with the specified fields in the hash stored at key.
		///     For every field that does not exist in the hash, a nil value is returned.
		///     Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a
		///     list of nil values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being requested.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys">Keys to retrieve from the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
		public async Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IEnumerable<string> keys,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return
				await m_cacheClient.HashGetAsync<T>(hashKey, keys.Select(key => GenerateKey(key)).AsEnumerable(), commandFlags);
		}

		/// <summary>
		///     Returns all fields and values of the hash stored at key. In the returned value,
		///     every field name is followed by its value, so the length of the reply is twice the size of the hash.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
		public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashGetAllAsync<T>(hashKey, commandFlags);
		}

		/// <summary>
		///     Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new key
		///     holding a hash is created.
		///     If field does not exist the value is set to 0 before the operation is performed.
		///     The range of values supported by HINCRBY is limited to 64 bit signed integers.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		public async Task<long> HashIncerementByAsync(string hashKey, string key, long value,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashIncerementByAsync(hashKey, GenerateKey(key), value, commandFlags);
		}

		/// <summary>
		///     Increment the specified field of an hash stored at key, and representing a floating point number, by the specified
		///     increment.
		///     If the field does not exist, it is set to 0 before performing the operation.
		/// </summary>
		/// <remarks>
		///     <para>
		///         An error is returned if one of the following conditions occur:
		///         * The field contains a value of the wrong type (not a string).
		///         * The current field content or the specified increment are not parsable as a double precision floating point
		///         number.
		///     </para>
		///     <para>
		///         Time complexity: O(1)
		///     </para>
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="value">the value at field after the increment operation</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value at field after the increment operation.</returns>
		public async Task<double> HashIncerementByAsync(string hashKey, string key, double value,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashIncerementByAsync(hashKey, GenerateKey(key), value, commandFlags);
		}

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashKeysAsync(hashKey, commandFlags);
		}

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		public async Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashLengthAsync(hashKey, commandFlags);
		}

		/// <summary>
		///     Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field
		///     already exists in the hash, it is overwritten.
		/// </summary>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="nx">Behave like hsetnx - set only if not exists</param>
		/// <param name="value">The value to be inserted</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     <c>true</c> if field is a new field in the hash and value was set.
		///     <c>false</c> if field already exists in the hash and no operation was performed.
		/// </returns>
		public async Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashSetAsync(hashKey, GenerateKey(key), value, nx, commandFlags);
		}

		/// <summary>
		///     Sets the specified fields to their respective values in the hash stored at key.
		///     This command overwrites any existing fields in the hash.
		///     If key does not exist, a new key holding a hash is created.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being set.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command executions flags</param>
		/// <param name="values"></param>
		public async Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values,
			CommandFlags commandFlags = CommandFlags.None)
		{
			await
				m_cacheClient.HashSetAsync(hashKey,
					values.Select(v => new {key = GenerateKey(v.Key), value = v.Value}).ToDictionary(c => c.key, c => c.value),
					commandFlags);
		}

		/// <summary>
		///     Returns all values in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
		public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashValuesAsync<T>(hashKey, commandFlags);
		}

		/// <summary>
		///     iterates fields of Hash types and their associated values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1) for every call. O(N) for a complete iteration, including enough command calls for the cursor
		///     to return back to 0.
		///     N is the number of elements inside the collection.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="pattern">GLOB search pattern</param>
		/// <param name="pageSize"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns></returns>
		public async Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10,
			CommandFlags commandFlags = CommandFlags.None)
		{
			return await m_cacheClient.HashScanAsync<T>(hashKey, pattern, pageSize, commandFlags);
		}

		private string GenerateKey(string key)
		{
			return string.Format("{0}_{1}", m_KeyPrefix, key);
		}

		#region [ Get And Fill ]

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="getData"></param>
		/// <returns></returns>
		public T GetAndFill<T>(string cacheKey, Func<T> getData)
		{
			return GetAndFill(cacheKey, getData, TimeSpan.FromMinutes(30));
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="getData"></param>
		/// <returns></returns>
		public async Task<T> GetAndFillAsync<T>(string cacheKey, Func<Task<T>> getData)
		{
			return await GetAndFillAsync(cacheKey, getData, TimeSpan.FromMinutes(30));
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="getData"></param>
		/// <param name="expiration"></param>
		/// <returns></returns>
		public T GetAndFill<T>(string cacheKey, Func<T> getData, TimeSpan expiration)
		{
			if (!isCachingEnabled)
			{
				return getData();
			}
			try
			{
				var data = Get<T>(cacheKey);
				if (data == null)
				{
					var cacheLogObject = lockerObject.GetOrAdd(cacheKey, new object());
					lock (cacheLogObject)
					{
						data = Get<T>(cacheKey);
						if (data == null)
						{
							data = getData();
							Add(cacheKey, data, expiration);
						}
					}
				}
				return data;
			}
			catch (Exception exc)
			{
				//TODO : Log
				throw exc;
			}
		}

		/// <summary>
		///     there is no lock and semaphore in this method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="getData"></param>
		/// <param name="expiration"></param>
		/// <returns></returns>
		public async Task<T> GetAndFillAsync<T>(string cacheKey, Func<Task<T>> getData, TimeSpan expiration)
		{
			//new System.Threading.Semaphore(1,1,)
			if (!isCachingEnabled)
			{
				return await getData();
			}
			try
			{
				var data = await GetAsync<T>(cacheKey);
				if (data == null)
				{
					data = await getData();
					Add(cacheKey, data, expiration);
				}
				return data;
			}
			catch (Exception exc)
			{
				//TODO : Log
				throw exc;
			}
		}

		#endregion
	}
}