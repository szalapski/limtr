using StackExchange.Redis;
using System;

namespace Limiter { 
    public class RedisLimitStore : ILimitStore {
        public RedisLimitStore(IDatabase redisDatabase, int hitLimitPerMinute) {
            // TODO: integrate with Azure?

            _hitLimit = hitLimitPerMinute;
            _limitInterval = TimeSpan.FromMinutes(1);
        }

        public RedisLimitStore(IDatabase redisDatabase, int hitLimit, TimeSpan interval)
        {
            _hitLimit = hitLimit;
            _limitInterval = interval;

        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);

        public bool Limit(string _appKey, string limitKey) {
            return true;
        }

        public void SetLimit(int hitsPerMinute) {
            throw new NotImplementedException();
        }
    }
}
