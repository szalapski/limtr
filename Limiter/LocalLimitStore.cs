using System;
using System.Collections.Generic;
using System.Linq;

namespace Limiter {
    public class LocalLimitStore : ILimitStore {

        public LocalLimitStore(int hitLimitPerMinute) {
            _hitLimit = hitLimitPerMinute;
            _limitInterval = TimeSpan.FromMinutes(1);
        }

        public LocalLimitStore(int hitLimit, TimeSpan interval) {
            _hitLimit = hitLimit;
            _limitInterval = interval;

        }
        private int _hitLimit;
        private TimeSpan _limitInterval = TimeSpan.FromMinutes(1);

<<<<<<< HEAD
=======

        // naive way
>>>>>>> 9e74e6d461be1778fb0bf1cf2c8702b07dd7a51f
        private static Dictionary<string, Queue<DateTime>> _storage = new Dictionary<string, Queue<DateTime>>();

        public bool Limit(string dummyAppKey, string limitKey) {
            Queue<DateTime> hits;
            bool success = _storage.TryGetValue(limitKey, out hits);
            bool result = false;
            if (success) {
                if (hits.Count >= _hitLimit && DateTime.Now - hits.ElementAt(hits.Count - _hitLimit) < _limitInterval) {
                    result = true;
                }
                hits.Enqueue(DateTime.Now);
            }
            else {
                _storage.Add(limitKey, new Queue<DateTime>(new[] { DateTime.Now }));
            }
            return result;
        }

        public void SetLimit(int hitsPerMinute) {
            _hitLimit = hitsPerMinute;
        }
    }
}
