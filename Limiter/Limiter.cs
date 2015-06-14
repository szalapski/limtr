using SzLimiter;

namespace SzLimiter {
    public class Limiter {
        public Limiter(string appKey, ILimitStore store) {
            _appKey = appKey;
            _store = store;
        }
        private ILimitStore _store { get; set; }
        private string _appKey {get; set;}

        /// <summary>
        /// Records a hit for the given limit key and returns a status.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string limitKey) {
            return !_store.Allows(_appKey, limitKey);
        }

        /// <summary>
        /// Records a hit for the given limit key, and throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string limitKey) {
            if (!Allows(limitKey)) throw new LimitReachedException();
        }

    }
}
