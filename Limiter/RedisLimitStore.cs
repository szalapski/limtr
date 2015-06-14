using StackExchange.Redis;
using System;

namespace SzLimiter { 
    public class RedisLimitStore : ILimitStore {
        public RedisLimitStore(IDatabase redisDatabase, int hitLimitPerMinute) {
            // TODO: integrate with Azure?

            _hitLimit = hitLimitPerMinute;
            _limitInterval = TimeSpan.FromMinutes(1);
        }

        public RedisLimitStore(int hitLimit, TimeSpan interval) {
            _hitLimit = hitLimit;
            _limitInterval = interval;

        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);

        public bool Limit(string _appKey, string limitKey) {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("foo");


            throw new NotImplementedException();
        }

        public void SetLimit(int hitsPerMinute) {
            throw new NotImplementedException();
        }

        public int GetLimit() {
            throw new NotImplementedException();
        }

    }
}
