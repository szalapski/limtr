using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Limtr.Lib { 
    public class RedisLimitStore : ILimitStore {

        public RedisLimitStore(IDatabase redisDatabase)
        {
            _database = redisDatabase;
        }
        private IDatabase _database;

        public bool Allows(string appKey, string bucket, string limitKey) {
            bool allowed = IsAllowed(appKey, bucket, limitKey);
            if (allowed){
                string key = MakeKey(appKey, bucket, limitKey);
                AddHit(key);       // A disallowed call does not count against the limit
            }
            return allowed;
        }

        public bool IsAllowed(string appKey, string bucket, string limitKey) {
            string key = MakeKey(appKey, bucket, limitKey);
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

        public static string MakeKey(string appKey, string bucket, string limitKey) {
            if (string.IsNullOrWhiteSpace(appKey)) appKey = "default";
            if (string.IsNullOrWhiteSpace(bucket)) appKey = "default";
            if (string.IsNullOrWhiteSpace(limitKey)) throw new InvalidOperationException("bad key");    //TODO: get rid of primitive obsession
            return string.Format("hits:{0}:{1}:{2}", appKey, bucket, limitKey);
        }
        public static string MakeBucketKeyPrefix(string appKey, string bucket) {
            return string.Format("buckets:{0}:{1}", appKey, bucket);
        }


        private void AddHit(string key) {
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
        }

        public void Setup(string appKey, string bucket, long hitLimit, TimeSpan limitInterval) {
            string bucketPrefix = MakeBucketKeyPrefix(appKey, bucket);
            _database.StringSet(string.Format("{0}:hitLimit", bucketPrefix), hitLimit);
            _database.StringSet(string.Format("{0}:limitInterval", bucketPrefix), limitInterval.Ticks);
        }
    }
}
