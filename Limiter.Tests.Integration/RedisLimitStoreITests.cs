﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
namespace SzLimiter.Tests.Integration
{
    [TestClass]
    public class RedisLimitStoreITests
    {
        private readonly ConnectionMultiplexer redis;

        public RedisLimitStoreITests()
        {
            var azureRedisOptions = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            var localRedisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            redis = ConnectionMultiplexer.Connect(localRedisOptions);
        }

        private const string appKey = "AzureLimitStoreTests_AppKey";

        [TestMethod]
        public void TestRedisConnection()
        {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit()
        {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db, 2);
            bool result = store.Allows(appKey, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, testLimitKey);
            store.Allows(appKey, testLimitKey);

            bool result = store.Allows(appKey, testLimitKey);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerHalfMinuteCallOnce_DoesNotLimit()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);

            bool result = store.Allows(appKey, Guid.NewGuid().ToString());

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerHalfMinuteCallThrice_Limits()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, testLimitKey);
            store.Allows(appKey, testLimitKey);
            bool result = store.Allows(appKey, testLimitKey);

            Assert.IsFalse(result);
        }
    }
}
