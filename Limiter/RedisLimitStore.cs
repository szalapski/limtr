using StackExchange.Redis;
using System;

namespace SzLimiter { 
    public class RedisLimitStore : ILimitStore {
        public RedisLimitStore(IDatabase redisDatabase, int hitLimitPerMinute) : this(redisDatabase, hitLimitPerMinute, TimeSpan.FromMinutes(1)) {
        }

        public RedisLimitStore(IDatabase redisDatabase, int hitLimit, TimeSpan interval)
        {
            _hitLimit = hitLimit;
            _limitInterval = interval;
            _database = redisDatabase;
        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);
        private IDatabase _database;

        public bool Allows(string appKey, string limitKey) {
            string key = MakeKey(appKey, limitKey);
            return Allows(key);
        }

        private bool Allows(string key) {
            RedisValue itemInQuestion = _database.ListGetByIndex(key, _hitLimit - 1);
            bool allowed = IsAllowed(key);
            if (allowed) AddHit(key);       // A disallowed call does not count against the limit
            return allowed;
        }

        public bool IsAllowed(string appKey, string limitKey) {
            string key = MakeKey(appKey, limitKey);
            return IsAllowed(key);
        }

        private bool IsAllowed(string key) {
            RedisValue itemInQuestion = _database.ListGetByIndex(key, _hitLimit - 1);
            return (!itemInQuestion.HasValue || DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion) > _limitInterval);
        }

        private static string MakeKey(string appKey, string limitKey) {
            if (string.IsNullOrWhiteSpace(appKey)) appKey = "default";
            if (string.IsNullOrWhiteSpace(limitKey)) throw new InvalidOperationException("bad key");    //TODO: get rid of primitive obsession
            return string.Format("{0}:{1}", appKey, limitKey);
        }


        private void AddHit(string key) {
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
        }
    }
}
