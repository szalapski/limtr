using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Diagnostics;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Integration {
    [TestClass]
    public class RedisLimitStoreITests_Local {
        private static ConnectionMultiplexer redis;
        private const string appKey = "RedisLimitStoreITests_Local";
        private const string bucket = "default";
        private const string quickBucket = "quick";

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            redis = ConnectionMultiplexer.Connect(redisOptions);

            var db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            store.SetupBucket(appKey, bucket, 2, TimeSpan.FromMinutes(1));
            store.SetupBucket(appKey, quickBucket, 2, TimeSpan.FromSeconds(1));
        }


        [TestMethod]
        public void TestRedisConnection() {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_For2PerMinuteCallOnce_DoesNotLimit() {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db);
            bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var store = new RedisLimitStore(redis.GetDatabase());
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            bool result = store.Allows(appKey, bucket, testLimitKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_QuickBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "First call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Third call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Fourth call");
            Assert.IsFalse(store.Allows(appKey, quickBucket, testLimitKey));
        }

        [TestMethod]
        public void SetupBucket_ForNewAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);

            //act
            store.SetupBucket(testAppKey, testBucket);
            bool isActive = store.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void SetupBucket_ForExistingAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            store.SetupBucket(testAppKey, testBucket);

            //act
            store.SetupBucket(testAppKey, testBucket);
            bool isActive = store.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void SetupBucket_ForNewAppKey_DefaultBucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);

            //act
            store.SetupBucket(testAppKey);
            bool isActive = store.IsActiveBucket(testAppKey);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void IsActiveBucket_ForMadeUpAppKey_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);

            //act
            bool isActive = store.IsActiveBucket(testAppKey);

            //assert
            Assert.IsFalse(isActive);
        }

        [TestMethod]
        public void IsActiveBucket_ForMadeUpAppKeyAndBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);

            //act
            bool isActive = store.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsFalse(isActive);
        }


        [TestMethod]
        public void IsActiveBucket_ForNewAppKeyAndMadeUpBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            store.SetupBucket(testAppKey);

            //act
            bool isActive = store.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsFalse(isActive);
        }


        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);
            System.Threading.Thread.Sleep(1000);
            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeHitKey(appKey, quickBucket, testLimitKey));
            Assert.AreEqual(2, items.Length);
        }

        [TestMethod]
        public void PerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine(db.Ping().TotalMilliseconds);

                var store = new RedisLimitStore(db);
                var sw = Stopwatch.StartNew();
                bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
                sw.Stop();
                Console.WriteLine("Allows: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }

    }
    [TestClass]
    public class RedisLimitStoreITests_Azure {
        private static ConnectionMultiplexer redis;

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            redis = ConnectionMultiplexer.Connect(redisOptions);

            var db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            store.SetupBucket(appKey, bucket, 2, TimeSpan.FromMinutes(1));
            store.SetupBucket(appKey, quickBucket, 2, TimeSpan.FromSeconds(1));
        }

        private const string appKey = "RedisLimitStoreITests_Azure";
        private const string bucket = "default";
        private const string quickBucket = "quick";

        [TestMethod]
        public void TestRedisConnection() {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var db = redis.GetDatabase();

            var store = new RedisLimitStore(db);
            bool result = store.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var store = new RedisLimitStore(redis.GetDatabase());
            string testLimitKey = Guid.NewGuid().ToString();
            store.Allows(appKey, bucket, testLimitKey);
            store.Allows(appKey, bucket, testLimitKey);

            bool result = store.Allows(appKey, bucket, testLimitKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "First call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Third call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testLimitKey), "Fourth call");
            Assert.IsFalse(store.Allows(appKey, quickBucket, testLimitKey));
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);
            System.Threading.Thread.Sleep(1000);
            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);
            store.Allows(appKey, quickBucket, testLimitKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeHitKey(appKey, quickBucket, testLimitKey));
            Assert.AreEqual(2, items.Length);
        }


        #region Performance Tests
        [TestMethod]
        public void AllowsPerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine("Ping {0}", db.Ping().TotalMilliseconds);

                var store = new RedisLimitStore(db);
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
                var store = new RedisLimitStore(db);
                string limitKey = Guid.NewGuid().ToString();
                store.Allows(appKey, bucket, limitKey);

                var sw = Stopwatch.StartNew();
                bool result = store.IsAllowed(appKey, bucket, limitKey);
                sw.Stop();
                Console.WriteLine("IsAllowed: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }
        #endregion
    }


}
