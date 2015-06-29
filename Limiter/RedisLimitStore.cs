using StackExchange.Redis;
using System;
using System.Diagnostics;
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
                Throttle(appKey, bucket, limitKey);
                AddHit(MakeHitKey(appKey, bucket, limitKey));       // A disallowed call does not count against the limit
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
            if (string.IsNullOrWhiteSpace(bucket)) bucket = "default";
            if (string.IsNullOrWhiteSpace(limitKey)) throw new InvalidOperationException("bad key");    //TODO: get rid of primitive obsession
            return string.Format("hits:{0}:{1}:{2}", appKey, bucket, limitKey);
        }
        private static string MakeBucketKeyPrefix(string appKey, string bucket) {
            return string.Format("buckets:{0}:{1}", appKey, bucket);
        }
        private static string MakeAppKeyPrefix(string appKey) {
            return string.Format("appKeys:{0}", appKey);
        }

        private void AddHit(string key) {
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
        }
        private void Throttle(string appKey, string bucket, string limitKey) {
            Stopwatch sw = Stopwatch.StartNew();
            long? throttleLimit = (long?)StringGet(MakeBucketKeyPrefix(appKey, bucket), "throttleLimit");
            if (throttleLimit.HasValue) {
                TimeSpan throttleInterval = TimeSpan.FromTicks((long)StringGet(MakeBucketKeyPrefix(appKey, bucket), "throttleInterval"));
                TimeSpan throttleDelay = TimeSpan.FromTicks((long)StringGet(MakeBucketKeyPrefix(appKey, bucket), "throttleDelay"));
                bool needsDelay = NeedsThrottle(MakeHitKey(appKey, bucket, limitKey), throttleLimit.Value, throttleInterval);
                sw.Stop();
                TimeSpan delayNeeded = throttleDelay - sw.Elapsed + TimeSpan.FromMilliseconds(1);
                if (needsDelay && delayNeeded > TimeSpan.Zero) System.Threading.Thread.Sleep(delayNeeded);
            }
        }
        private bool NeedsThrottle(string key, long throttleLimit, TimeSpan throttleInterval) {
            RedisValue itemInQuestion = _database.ListGetByIndex(key, throttleLimit - 1);
            if (itemInQuestion.HasValue) {
                TimeSpan elapsed = DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion);
                if (elapsed < throttleInterval) return true;
            }
            return false;
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

        public void Setup(Bucket bucket) {
            string bucketPrefix = MakeBucketKeyPrefix(bucket.AppKey, bucket.Name);
            StringSetTo(true, MakeAppKeyPrefix(bucket.AppKey), "isActive");
            StringSetTo(true, bucketPrefix, "isActive");
            StringSetTo(bucket.HitLimit, bucketPrefix, "hitLimit");
            StringSetTo(bucket.LimitInterval.Ticks, bucketPrefix, "limitInterval");
            if (bucket.ThrottleLimit.HasValue && bucket.ThrottleInterval.HasValue) {
                StringSetTo(bucket.ThrottleLimit, bucketPrefix, "throttleLimit");
                StringSetTo(bucket.ThrottleInterval.Value.Ticks, bucketPrefix, "throttleInterval");
                StringSetTo(bucket.ThrottleDelay.Value.Ticks, bucketPrefix, "throttleDelay");
            }
        }

        public Bucket LoadBucket(string appKey, string name = null) {
            string bucketPrefix = MakeBucketKeyPrefix(appKey, name);
            bool found = (bool)StringGet(bucketPrefix, "isActive");
            if (!found) return null;
            long? throttleIntervalTicks = (long?)StringGet(bucketPrefix, "throttleInterval");
            long? throttleDelayTicks = (long?)StringGet(bucketPrefix, "throttleDelay");

            return new Bucket(
                appKey,
                name,
                (long)StringGet(bucketPrefix, "hitLimit"),
                TimeSpan.FromTicks((long)StringGet(bucketPrefix, "limitInterval")),
                (long?)StringGet(bucketPrefix, "throttleLimit"),
                throttleIntervalTicks.HasValue ? TimeSpan.FromTicks(throttleIntervalTicks.Value) : default(TimeSpan?),
                throttleDelayTicks.HasValue ? TimeSpan.FromTicks(throttleDelayTicks.Value) : default(TimeSpan?)
            );
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
