using StackExchange.Redis;
using System;

namespace Limiter { 
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
            string key = string.Format("{0}:{1}", appKey, limitKey);
            RedisValue itemInQuestion = _database.ListGetByIndex(key, _hitLimit);
            if (itemInQuestion.HasValue && DateTime.UtcNow - DateTime.FromFileTimeUtc((long)itemInQuestion) < _limitInterval)
            {
                _database.ListLeftPush(key, DateTime.Now.ToFileTimeUtc());
                return false;
            }
            else return true;

        }

    }
}
