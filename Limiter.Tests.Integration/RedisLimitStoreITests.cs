using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Diagnostics;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Integration {
    [TestClass]
    public class RedisLimitStoreITests_Local {
        private readonly ConnectionMultiplexer redis;

        public RedisLimitStoreITests_Local() {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            redis = ConnectionMultiplexer.Connect(redisOptions);
        }

        private const string appKey = "RedisLimitStoreITests_Local";
        private const string bucket = "default";

        [TestMethod]
        public void TestRedisConnection() {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_For2PerMinuteCallOnce_DoesNotLimit() {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db, 2);
            bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            bool result = store.Allows(appKey, bucket, testLimitKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db, 2, TimeSpan.FromSeconds(1));
            string testLimitKey = Guid.NewGuid().ToString();

            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "First call");
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Third call");
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Fourth call");
            Assert.IsFalse(store.Allows(appKey, bucket, testLimitKey));
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db, 2, TimeSpan.FromSeconds(1));
            string testLimitKey = Guid.NewGuid().ToString();

            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);
            System.Threading.Thread.Sleep(1000);
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeKey(appKey, bucket, testLimitKey));
            Assert.AreEqual(2, items.Length);
        }

        [TestMethod]
        public void PerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine(db.Ping().TotalMilliseconds);

                var store = new RedisLimitStore(db, 2);
                var sw = Stopwatch.StartNew();
                bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
                sw.Stop();
                Console.WriteLine("Allows: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }

    }
    [TestClass]
    public class RedisLimitStoreITests_Azure {
        private readonly ConnectionMultiplexer redis;

        public RedisLimitStoreITests_Azure() {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            redis = ConnectionMultiplexer.Connect(redisOptions);
        }

        private const string appKey = "RedisLimitStoreITests_Azure";
        private const string bucket = "default";

        [TestMethod]
        public void TestRedisConnection() {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db, 2);
            bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var store = new RedisLimitStore(redis.GetDatabase(), 2);
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            bool result = store.Allows(appKey, bucket, testLimitKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db, 2, TimeSpan.FromSeconds(1));
            string testLimitKey = Guid.NewGuid().ToString();

            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "First call");
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Third call");
            Assert.IsTrue(store.Allows(appKey, bucket, testLimitKey), "Fourth call");
            Assert.IsFalse(store.Allows(appKey, bucket, testLimitKey));
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db, 2, TimeSpan.FromSeconds(1));
            string testLimitKey = Guid.NewGuid().ToString();

            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);
            System.Threading.Thread.Sleep(1000);
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeKey(appKey, bucket, testLimitKey));
            Assert.AreEqual(2, items.Length);
        }

        [TestMethod]
        public void AllowsPerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine("Ping {0}", db.Ping().TotalMilliseconds);

                var store = new RedisLimitStore(db, 2);
                var sw = Stopwatch.StartNew();
                bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
                sw.Stop();
                Console.WriteLine("Allows: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }

        [TestMethod]
        public void IsAllowedPerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine("Ping {0}", db.Ping().TotalMilliseconds);
                var store = new RedisLimitStore(db, 2);
                string limitKey = Guid.NewGuid().ToString();
                store.Allows(appKey, bucket, limitKey);

                var sw = Stopwatch.StartNew();
                bool result = store.IsAllowed(appKey, bucket, limitKey);
                sw.Stop();
                Console.WriteLine("IsAllowed: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }

    }


}
