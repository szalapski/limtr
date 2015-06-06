using System;

namespace Limiter { 
    public class AzureLimitStore : ILimitStore {
        public AzureLimitStore(int hitLimitPerMinute) {
            _hitLimit = hitLimitPerMinute;
            _limitInterval = TimeSpan.FromMinutes(1);
        }

        public AzureLimitStore(int hitLimit, TimeSpan interval) {
            _hitLimit = hitLimit;
            _limitInterval = interval;

        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);

        public bool Limit(string _appKey, string limitKey) {
            throw new NotImplementedException();
        }

        public void SetLimit(int hitsPerMinute) {
            throw new NotImplementedException();
        }
    }
}
