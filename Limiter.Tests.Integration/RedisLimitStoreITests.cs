using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Diagnostics;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Integration {
    [TestClass]  
    public class RedisLimitStoreITests {
        private static Redis redis;
        private const string appKey = "RedisLimitStoreITests_Local";
        private const string bucket = "default";
        private const string quickBucket = "quick";

        [ClassInitialize]
        public static void InitializeClass(TestContext context) {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false, AllowAdmin = true };
            redis = new Redis(ConnectionMultiplexer.Connect(redisOptions));
            var sut = new RedisLimitStore(redis);

            foreach (IServer server in redis.Servers) server.FlushAllDatabases();

            sut.Setup(new Bucket(appKey, bucket, 2, TimeSpan.FromMinutes(1)));
            sut.Setup(new Bucket(appKey, quickBucket, 2, TimeSpan.FromSeconds(1)));
        }



        [TestMethod]
        public void TestRedisConnection() {
            int[] xxx = { 1, 2 };
            var db = redis.Database;
            Console.WriteLine(db.Ping());
        }

        [TestMethod]
        public void Allows_For2PerMinuteCallOnce_DoesNotLimit() {
            var db = redis.Database;

            var sut = new RedisLimitStore(redis);
            bool result = sut.Allows(appKey, bucket, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var sut = new RedisLimitStore(redis);
            string testOpKey = Guid.NewGuid().ToString();
            sut.Allows(appKey, bucket, testOpKey);
            sut.Allows(appKey, bucket, testOpKey);

            bool result = sut.Allows(appKey, bucket, testOpKey);

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Allows_QuickBucketFor2PerSecondCallFiveTimesWithGapAfter2_AllowsFirstFour() {
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            string testOpKey = Guid.NewGuid().ToString();

            Assert.IsTrue(sut.Allows(appKey, quickBucket, testOpKey), "First call");
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testOpKey), "Second call");
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testOpKey), "Third call");
            Assert.IsTrue(sut.Allows(appKey, quickBucket, testOpKey), "Fourth call");
            Assert.IsFalse(sut.Allows(appKey, quickBucket, testOpKey));
        }

        [TestMethod]
        public void Setup_ForNewAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucketName = "testBucketName1";
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);

            //act
            sut.Setup(new Bucket(testAppKey, testBucketName));
       
            //assert (throws if not exist)
            sut.LoadBucket(testAppKey, testBucketName);
        }

        [TestMethod]
        public void Setup_ForExistingAppKeyAndBucket_BucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucketName = "testBucketName1";
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            sut.Setup(new Bucket(testAppKey, testBucketName));

            //act
            sut.Setup(new Bucket(testAppKey, testBucketName));

            //assert (throws if not exist)
            sut.LoadBucket(testAppKey, testBucketName);
        }

        [TestMethod]
        public void Setup_ForNewAppKey_DefaultBucketExists() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);

            //act
            sut.Setup(new Bucket(testAppKey));

            //assert (throws if not exist)
            sut.LoadBucket(testAppKey);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadBucket_ForMadeUpAppKey_ThrowsException() {
            string testAppKey = Guid.NewGuid().ToString();
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);

            //act
            sut.LoadBucket(testAppKey);

            //assert - throws 
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadBucket_ForMadeUpAppKeyAndBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucketName = "testBucketName1";
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
                        
            //act
            sut.LoadBucket(testAppKey, testBucketName);

            //assert (throws)
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadBucket_ForNewAppKeyAndMadeUpBucket_ReturnsFalse() {
            string testAppKey = Guid.NewGuid().ToString();
            string testBucketName = "testBucketName1";
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            sut.Setup(new Bucket(testAppKey));


            //act
            sut.LoadBucket(testAppKey, testBucketName);

            //assert (throws)
        }


        [TestMethod]
        public void Allows_DefaultBucketFor2PerSecondCallFiveTimesWithGap_TrimsListTo2() {
            //This test sometimes fails--I believe because the async op to trim the list can take a few seconds.

            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            string testOpKey = Guid.NewGuid().ToString();

            sut.Allows(appKey, quickBucket, testOpKey);
            sut.Allows(appKey, quickBucket, testOpKey);
            System.Threading.Thread.Sleep(1000);
            sut.Allows(appKey, quickBucket, testOpKey);
            sut.Allows(appKey, quickBucket, testOpKey);
            sut.Allows(appKey, quickBucket, testOpKey);

            RedisValue[] items = db.ListRange(RedisLimitStore.MakeHitKey(appKey, quickBucket, testOpKey));
            Assert.AreEqual(2, items.Length);
        }
        [TestMethod]
        public void Allows_ForSetupThrottledBucketWithLimitAndThrottle_Throttles() {
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.Setup(new Bucket(appKey, bucket, 3, TimeSpan.FromSeconds(60), 2, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1)));
            sut.Allows(appKey, bucket, operation);
            sut.Allows(appKey, bucket, operation);

            //act
            var sw = Stopwatch.StartNew();
            bool result = sut.Allows(appKey, bucket, operation);
            sw.Stop();

            //assert
            Console.WriteLine(sw.Elapsed);
            Assert.IsTrue(sw.Elapsed > TimeSpan.FromSeconds(1), "elapsed was too quick; throttle seems to be ineffective");
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_ForSetupThrottledBucketWithLimitAndThrottle_Limits() {
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.Setup(new Bucket(appKey, bucket, 3, TimeSpan.FromSeconds(60), 2, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1)));
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
            IDatabase db = redis.Database;
            var sut = new RedisLimitStore(redis);
            string appKey = Guid.NewGuid().ToString(), bucket = "test", operation = "op1";
            sut.Setup(new Bucket(appKey, bucket, 2, TimeSpan.FromSeconds(60)));
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
                var db = redis.Database;

                Console.WriteLine(db.Ping().TotalMilliseconds);

                var sut = new RedisLimitStore(redis);
                var sw = Stopwatch.StartNew();
                bool result = sut.Allows(appKey, bucket, Guid.NewGuid().ToString());
                sw.Stop();
                Console.WriteLine("Allows: {0}", sw.Elapsed.TotalMilliseconds);
            }
        }



    }


}
