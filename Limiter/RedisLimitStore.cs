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

        public bool Limit(string appKey, string limitKey) {
            _database.Ping();
            string key = string.Format("{0}:{1}", appKey, limitKey);
            key = limitKey.Substring(0, 4);
            RedisValue itemInQuestion = _database.ListGetByIndex(key, _hitLimit-1);
            if (itemInQuestion.HasValue && DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion) < _limitInterval)
            {
                // A disallowed call does not count against the limit
                return true;
            }
            _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
            return false;

        }

    }
}
