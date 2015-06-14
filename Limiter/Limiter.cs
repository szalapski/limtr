using SzLimiter;

namespace SzLimiter {
    public class Limiter {
        public Limiter(string appKey, ILimitStore store) {
            _appKey = appKey;
            _store = store;
        }
        private ILimitStore _store { get; set; }
        private string _appKey {get; set;}

        public bool Limit(string limitKey) {
            return _store.Limit(_appKey, limitKey);
        }

    }
}
