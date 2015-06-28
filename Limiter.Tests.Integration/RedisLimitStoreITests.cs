using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Diagnostics;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Integration {
    [TestClass]
    public class RedisLimitStoreITests {
        private static ConnectionMultiplexer redis;
        private const string appKey = "RedisLimitStoreITests_Local";
        private const string bucket = "default";
        private const string quickBucket = "quick";

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false, AllowAdmin = true };
            redis = ConnectionMultiplexer.Connect(redisOptions);

            var db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);

            foreach (System.Net.EndPoint endpoint in redis.GetEndPoints()) redis.GetServer(endpoint).FlushAllDatabases();

            sut.SetupBucket(appKey, bucket, 2, TimeSpan.FromMinutes(1));
            sut.SetupBucket(appKey, quickBucket, 2, TimeSpan.FromSeconds(1));
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

            var sut = new RedisLimitStore(db);
            bool result = sut.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var sut = new RedisLimitStore(redis.GetDatabase());
            string testLimitKey = Guid.NewGuid().ToString();
            sut.Allows(appKey, bucket, testLimitKey);
            sut.Allows(appKey, bucket, testLimitKey);

            bool result = sut.Allows(appKey, bucket, testLimitKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_QuickBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            Assert.IsTrue(sut.Allows(appKey, quickBucket, testLimitKey), "First call");
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testLimitKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testLimitKey), "Third call");
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testLimitKey), "Fourth call");
            Assert.IsFalse(sut.Allows(appKey, quickBucket, testLimitKey));
        }

        [TestMethod]
        public void SetupBucket_ForNewAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);

            //act
            sut.SetupBucket(testAppKey, testBucket);
            bool isActive = sut.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void SetupBucket_ForExistingAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            sut.SetupBucket(testAppKey, testBucket);

            //act
            sut.SetupBucket(testAppKey, testBucket);
            bool isActive = sut.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void SetupBucket_ForNewAppKey_DefaultBucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);

            //act
            sut.SetupBucket(testAppKey);
            bool isActive = sut.IsActiveBucket(testAppKey);

            //assert
            Assert.IsTrue(isActive);
        }

        [TestMethod]
        public void IsActiveBucket_ForMadeUpAppKey_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);

            //act
            bool isActive = sut.IsActiveBucket(testAppKey);

            //assert
            Assert.IsFalse(isActive);
        }

        [TestMethod]
        public void IsActiveBucket_ForMadeUpAppKeyAndBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);

            //act
            bool isActive = sut.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsFalse(isActive);
        }


        [TestMethod]
        public void IsActiveBucket_ForNewAppKeyAndMadeUpBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucket = "testBucket1";
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            sut.SetupBucket(testAppKey);

            //act
            bool isActive = sut.IsActiveBucket(testAppKey, testBucket);

            //assert
            Assert.IsFalse(isActive);
        }


        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            string testLimitKey = Guid.NewGuid().ToString();

            sut.Allows(appKey, quickBucket, testLimitKey);
            sut.Allows(appKey, quickBucket, testLimitKey);
            System.Threading.Thread.Sleep(1000);
            sut.Allows(appKey, quickBucket, testLimitKey);
            sut.Allows(appKey, quickBucket, testLimitKey);
            sut.Allows(appKey, quickBucket, testLimitKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeHitKey(appKey, quickBucket, testLimitKey));
            Assert.AreEqual(2, items.Length);
        }
        [TestMethod]
        public void Allows_ForSetupThrottledBucketWithLimitAndThrottle_Throttles() {
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.SetupThrottledBucket(appKey, bucket, 3, TimeSpan.FromSeconds(60), 2, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1));
            sut.Allows(appKey, bucket, operation);
            sut.Allows(appKey, bucket, operation);

            //act
            var sw = Stopwatch.StartNew();
            bool result = sut.Allows(appKey, bucket, operation);
            sw.Stop();

            //assert
            Assert.IsTrue(sw.ElapsedMilliseconds > 1000, "elapsed was too quick; throttle seems to be ineffective");
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_ForSetupThrottledBucketWithLimitAndThrottle_Limits() {
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.SetupThrottledBucket(appKey, bucket, 3, TimeSpan.FromSeconds(60), 2, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1));
            sut.Allows(appKey, bucket, operation);
            sut.Allows(appKey, bucket, operation);
            sut.Allows(appKey, bucket, operation);

            //act
            bool result = sut.Allows(appKey, bucket, operation);

            //assert
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_ForSetupThrottledBucketWithLimitAndWithoutThrottle_LimitsWithoutThrottle() {
            IDatabase db = redis.GetDatabase();
            var sut = new RedisLimitStore(db);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.SetupThrottledBucket(appKey, bucket, 2, TimeSpan.FromSeconds(60));
            sut.Allows(appKey, bucket, operation);
            sut.Allows(appKey, bucket, operation);

            //act
            var sw = Stopwatch.StartNew();
            bool result = sut.Allows(appKey, bucket, operation);
            sw.Stop();

            //assert
            Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
            Assert.IsFalse(result);
        }



        [TestMethod]
        public void PerformanceTest() {
            for (int i = 0; i < 10; i++) {
                var db = redis.GetDatabase();

                Console.WriteLine(db.Ping().TotalMilliseconds);

                var sut = new RedisLimitStore(db);
                var sw = Stopwatch.StartNew();
                bool result = sut.Allows(appKey, bucket, Guid.NewGuid().ToString());
                sw.Stop();
                Console.WriteLine("Allows: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }

    }


}
