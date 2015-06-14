using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SzLimiter;

namespace SzLimiter
{
    public class Limiter
    {

        // bastard constructor for public API
        public Limiter(string appKey) {
            Initialize(appKey);
        }

        public Limiter(string appKey, ILimitStore store) {
            _store = store;
            Initialize(appKey);
        }
        private ILimitStore _store { get; set; }
        
        private void Initialize(string appKey){ 
            _appKey = appKey;
        }
        private string _appKey {get; set;}

        public bool Limit(string limitKey) {
            return _store.Limit(_appKey, limitKey);
        }

        public void SetLimit(int hitsPerMinute){
            _store.SetLimit(hitsPerMinute);
        }


        // TDD 
        public int GetLimitPerMinute() {
            if (_store == null) throw new InvalidOperationException("Limiter does not have a backing store.");
            int result = _store.GetLimit();
            return result <= 1024 ? result : 1024;
        }

    }
}
