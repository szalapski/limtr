using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limiter;

namespace SzLimiter
{
    public class Limiter
    {

        // bastard constructor for public API
        public Limiter(string appKey) {
            Initialize(appKey);
        }

        public Limiter(string appKey, ILimitStore _store) {
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

    }
}
