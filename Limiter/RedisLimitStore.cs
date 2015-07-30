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

        public bool Allows(string appKey, string bucketName, string operationKey) {
            Bucket bucket = LoadBucket(appKey, bucketName);
            bool allowed = IsAllowed(bucket, operationKey);
            if (allowed) {
                Throttle(bucket, operationKey);
                AddHit(bucket.HitKeyFor(operationKey));       // A disallowed call does not count against the limit
            }
            return allowed;
        }

        public bool IsAllowed(string appKey, string bucketName, string operationKey) {
            return IsAllowed(LoadBucket(appKey, bucketName), operationKey);
        }

        private bool IsAllowed(Bucket bucket, string operationKey) {
            string key = bucket.HitKeyFor(operationKey);
            RedisValue itemInQuestion = _database.ListGetByIndex(key, bucket.HitLimit - 1);
            if (itemInQuestion.HasValue) {
                Task.Run(() => _database.ListTrim(key, 0, bucket.HitLimit - 1));   // trim the list on a different thread - no need to wait  (todo: should be server op?)
                TimeSpan elapsed = DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion);
                if (elapsed <= bucket.LimitInterval) return false;
            }
            return true;
        }

        public static string MakeHitKey(string appKey, string bucket, string operationKey) {
            if (string.IsNullOrWhiteSpace(appKey)) appKey = "default";
            if (string.IsNullOrWhiteSpace(bucket)) bucket = "default";
            if (string.IsNullOrWhiteSpace(operationKey)) throw new InvalidOperationException("bad key");    //TODO: get rid of primitive obsession
            return $"hits:{appKey}:{bucket}:{operationKey}";
        }

        private void AddHit(string key) {
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
        }

        private void Throttle(Bucket bucket, string operationKey) {
            Stopwatch sw = Stopwatch.StartNew();
            if (!bucket.Throttles) return; 
            bool needsDelay = NeedsThrottle(bucket, operationKey);
            sw.Stop();
            TimeSpan delayNeeded = bucket.ThrottleDelay.Value - sw.Elapsed + TimeSpan.FromMilliseconds(1);
            if (needsDelay && delayNeeded > TimeSpan.Zero) System.Threading.Thread.Sleep(delayNeeded);
        }
        private bool NeedsThrottle(Bucket bucket, string operationKey) {
            if (!bucket.Throttles) return false;
            RedisValue itemInQuestion = _database.ListGetByIndex(bucket.HitKeyFor(operationKey), bucket.ThrottleLimit.Value - 1);
            if (itemInQuestion.HasValue) {
                TimeSpan elapsed = DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion);
                if (elapsed < bucket.ThrottleInterval) return true;
            }
            return false;
        }

        /*
        public bool IsActiveAppKey(string appKey) {
            if (appKey == null) throw new ArgumentNullException("appKey");
            RedisValue appKeyIsActive = StringGet(MakeAppKeyPrefix(appKey), "isActive");
            return (bool)appKeyIsActive;
        }

        public bool IsActiveBucket(string appKey, string bucketName = null) {
            if (appKey == null) throw new ArgumentNullException("appKey");
            if (bucketName == null) bucketName = "default";

            RedisValue appKeyIsActive = StringGet(MakeAppKeyPrefix(appKey), "isActive");
            if (!(bool)appKeyIsActive) return false;
            RedisValue bucketIsActive = StringGet(MakeBucketKeyPrefix(appKey, bucketName), "isActive");
            return (bool)bucketIsActive;
        }
        */
        public void Setup(Bucket bucket) {
            // TODO: optimize for redis speed

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
            if (name == null) name = "default";
            Bucket result = TryLoadBucket(appKey, name);
            if (result == null) throw new InvalidOperationException($"AppKey '{appKey}' or bucket '{name}' hasn't been setup.");
            return result;
        }

        private Bucket TryLoadBucket(string appKey, string name = null) {
            // TODO: optimize for redis speed

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

        private static string MakeAppKeyPrefix(string appKey) {
            return $"appKeys:{appKey}";
        }
        private static string MakeBucketKeyPrefix(string appKey, string bucket) {
            return $"buckets:{appKey}:{bucket}";
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
