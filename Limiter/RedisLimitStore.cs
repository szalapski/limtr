using StackExchange.Redis;
using System;

namespace Limiter { 
    public class RedisLimitStore : ILimitStore {
        public RedisLimitStore(IDatabase redisDatabase, int hitLimitPerMinute) {
            // TODO: integrate with Azure?

            _hitLimit = hitLimitPerMinute;
            _limitInterval = TimeSpan.FromMinutes(1);
        }

<<<<<<< HEAD
        public RedisLimitStore(IDatabase redisDatabase, int hitLimit, TimeSpan interval)
        {
=======
        public RedisLimitStore(int hitLimit, TimeSpan interval) {
>>>>>>> 9e74e6d461be1778fb0bf1cf2c8702b07dd7a51f
            _hitLimit = hitLimit;
            _limitInterval = interval;

        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);

        public bool Limit(string _appKey, string limitKey) {
<<<<<<< HEAD
            
=======
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("foo");


            throw new NotImplementedException();
>>>>>>> 9e74e6d461be1778fb0bf1cf2c8702b07dd7a51f
        }

        public void SetLimit(int hitsPerMinute) {
            throw new NotImplementedException();
        }
    }
}
