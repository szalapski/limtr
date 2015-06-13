using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
namespace Limiter.Tests.Integration
{
    [TestClass]
    public class RedisLimitStoreITests
    {
        private ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(new ConfigurationOptions() {
            EndPoints = { { "limtr.redis.cache.windows.net", 6379 } },
            Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=",
        });
        private const string appKey = "AzureLimitStoreTests_AppKey";
       

        [TestMethod]
        public void TestRedisConnection()
        {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit()
        {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db, 2);
            bool result = store.Limit(appKey, Guid.NewGuid().ToString());
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallThrice_Limits()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Limit(appKey, testLimitKey);
            store.Limit(appKey, testLimitKey);

            bool result = store.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallOnce_DoesNotLimit()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);

            bool result = store.Limit(appKey, Guid.NewGuid().ToString());

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallThrice_Limits()
        {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Limit(appKey, testLimitKey);
            store.Limit(appKey, testLimitKey);
            bool result = store.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }
    }
}
