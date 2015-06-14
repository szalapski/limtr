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
        /// If the operation represented by the limit key is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string limitKey) {
            return _store.Allows(_appKey, limitKey);
        }

        /// <summary>
        /// Peeks at whether the operation represented by the limit key is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool IsAllowed(string limitKey) {
            return _store.IsAllowed(_appKey, limitKey);
        }

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit; throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string limitKey) {
            if (!Allows(limitKey)) throw new LimitReachedException();
        }

    }
}
