using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Diagnostics;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Integration {
 
   // [TestClass]   // generally disabled to avoid too much traffic to Azure
    public class RedisLimitStoreITests_Azure {
        private static ConnectionMultiplexer redis;

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            redis = ConnectionMultiplexer.Connect(redisOptions);

            var db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            store.Setup(new Bucket(appKey, bucket, 2, TimeSpan.FromMinutes(1)));
            store.Setup(new Bucket(appKey, quickBucket, 2, TimeSpan.FromSeconds(1)));
            //store.Setup(new Bucket("free", null, 10, TimeSpan.FromSeconds(10), 7, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200)));
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
        public void TestRedisFreeApp() {
            int[] xxx = { 1, 2 };
            var db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            // TODO
            //Assert.IsTrue(store.IsActiveAppKey("free"), "AppKey not found");
           // Assert.IsTrue(store.IsActiveBucket("free","default"), "bucket not found");
            

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
            string testOpKey = Guid.NewGuid().ToString();
            store.Allows(appKey, bucket, testOpKey);
            store.Allows(appKey, bucket, testOpKey);

            bool result = store.Allows(appKey, bucket, testOpKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testOpKey = Guid.NewGuid().ToString();

            Assert.IsTrue(store.Allows(appKey, quickBucket, testOpKey), "First call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testOpKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(store.Allows(appKey, quickBucket, testOpKey), "Third call");
            Assert.IsTrue(store.Allows(appKey, quickBucket, testOpKey), "Fourth call");
            Assert.IsFalse(store.Allows(appKey, quickBucket, testOpKey));
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var store = new RedisLimitStore(db);
            string testOpKey = Guid.NewGuid().ToString();

            store.Allows(appKey, quickBucket, testOpKey);
            store.Allows(appKey, quickBucket, testOpKey);
            System.Threading.Thread.Sleep(1000);
            store.Allows(appKey, quickBucket, testOpKey);
            store.Allows(appKey, quickBucket, testOpKey);
            store.Allows(appKey, quickBucket, testOpKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeHitKey(appKey, quickBucket, testOpKey));
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
