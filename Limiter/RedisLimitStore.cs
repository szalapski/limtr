using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Limtr.Lib {
    public class RedisLimitStore : ILimitStore {

        public RedisLimitStore(IDatabase redisDatabase) {
            _database = redisDatabase;
        }
        private IDatabase _database;

        public bool Allows(string appKey, string bucket, string limitKey) {
            bool allowed = IsAllowed(appKey, bucket, limitKey);
            if (allowed) {
                string key = MakeHitKey(appKey, bucket, limitKey);
                AddHit(key);       // A disallowed call does not count against the limit
            }
            return allowed;
        }

        public bool IsAllowed(string appKey, string bucket, string limitKey) {
            string key = MakeHitKey(appKey, bucket, limitKey);
            string bucketPrefix = MakeBucketKeyPrefix(appKey, bucket);
            // TODO: memoize
            long limit = (long)_database.StringGet(string.Format("{0}:hitLimit", bucketPrefix));
            var limitInterval = TimeSpan.FromTicks(
                (long)_database.StringGet(string.Format("{0}:limitInterval", bucketPrefix)));
            return IsAllowed(key, limit, limitInterval);
        }

        private bool IsAllowed(string key, long hitLimit, TimeSpan limitInterval) {
            RedisValue itemInQuestion = _database.ListGetByIndex(key, hitLimit - 1);
            if (itemInQuestion.HasValue) {
                Task.Run(() => _database.ListTrim(key, 0, hitLimit - 1));   // trim the list on a different thread - no need to wait  (todo: should be server op?)
                TimeSpan elapsed = DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion);
                if (elapsed <= limitInterval) return false;
            }
            return true;
        }

        public static string MakeHitKey(string appKey, string bucket, string limitKey) {
            if (string.IsNullOrWhiteSpace(appKey)) appKey = "default";
            if (string.IsNullOrWhiteSpace(bucket)) appKey = "default";
            if (string.IsNullOrWhiteSpace(limitKey)) throw new InvalidOperationException("bad key");    //TODO: get rid of primitive obsession
            return string.Format("hits:{0}:{1}:{2}", appKey, bucket, limitKey);
        }
        public static string MakeBucketKeyPrefix(string appKey, string bucket) {
            return string.Format("buckets:{0}:{1}", appKey, bucket);
        }
        public static string MakeAppKeyPrefix(string appKey) {
            return string.Format("appKeys:{0}", appKey);
        }

        private void AddHit(string key) {
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
        }

        public bool IsActiveAppKey(string appKey) {
            if (appKey == null) throw new ArgumentNullException("appKey");
            RedisValue appKeyIsActive = StringGet(MakeAppKeyPrefix(appKey), "isActive");
            return (bool)appKeyIsActive;
        }


        public bool IsActiveBucket(string appKey, string bucket = null) {
            if (appKey == null) throw new ArgumentNullException("appKey");
            if (bucket == null) bucket = "default";

            RedisValue appKeyIsActive = StringGet(MakeAppKeyPrefix(appKey), "isActive");
            if (!(bool)appKeyIsActive) return false;
            RedisValue bucketIsActive = StringGet(MakeBucketKeyPrefix(appKey, bucket), "isActive");
            return (bool)bucketIsActive;
        }

        /// <summary>
        /// Creates or modifies a bucket setup and/or an appkey setup with limit
        /// </summary>
        public void SetupBucket(string appKey, string bucket = null, long hitLimit = 10, TimeSpan limitInterval = default(TimeSpan)) {
            SetupThrottledBucket(appKey, bucket, hitLimit, limitInterval, null, null, null);
        }

        /// <summary>
        /// Creates or modifies a bucket setup and/or an appkey setup with limit and throttle
        /// </summary>
        public void SetupThrottledBucket(string appKey, string bucket = null,
                long hitLimit = 10, TimeSpan limitInterval = default(TimeSpan),
                long? throttleLimit = null, TimeSpan? throttleInterval = null, TimeSpan? throttleDelay = null) {
           // TODO: this needs a fluent interface 

            if (appKey == null) throw new ArgumentNullException("appKey");
            if (string.IsNullOrWhiteSpace(appKey)) throw new ArgumentException("The appKey must have a non-whitespace value", appKey);
            if (bucket == null) bucket = "default";
            if (throttleLimit.HasValue) {
                if (throttleLimit <= 0) throw new ArgumentOutOfRangeException("throttleLimit");
                if (!throttleInterval.HasValue) throw new ArgumentException("throttleInterval must be supplied if throttleLimit is supplied.", "throttleInterval");
                if (!throttleDelay.HasValue) throw new ArgumentException("throttleDelay must be supplied if throttleLimit is supplied.", "throttleDelay");
                if (throttleInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException("throttleInterval");
            }
            string bucketPrefix = MakeBucketKeyPrefix(appKey, bucket);
            StringSetTo(true, MakeAppKeyPrefix(appKey), "isActive");
            StringSetTo(true, bucketPrefix, "isActive");
            StringSetTo(hitLimit, bucketPrefix, "hitLimit");
            StringSetTo(limitInterval.Ticks, bucketPrefix, "limitInterval");
            if (throttleLimit.HasValue && throttleInterval.HasValue) {
                StringSetTo(throttleLimit, bucketPrefix, "throttleLimit");
                StringSetTo(throttleInterval.Value.Ticks, bucketPrefix, "throttleInterval");
                StringSetTo(throttleDelay.Value.Ticks, bucketPrefix, "throttleDelay");
            }
        }


        private static string Join(params string[] values) {
            return string.Join(":", values);
        }
        private bool StringSetTo(RedisValue value, params string[] redisKeyParts) {
            return _database.StringSet(Join(redisKeyParts), value);
        }
        private RedisValue StringGet(params string[] redisKeyParts) {
            return _database.StringGet(Join(redisKeyParts));
        }
    }
}
