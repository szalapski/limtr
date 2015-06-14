using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limtr
{
    public class Limiter
    {
         public Limiter(string appKey) {
            _appKey = appKey;
        }
        private string _appKey {get; set;}

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string limitKey) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Peeks at whether the operation represented by the limit key is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool IsAllowed(string limitKey) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit; throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string limitKey) {
            throw new NotImplementedException();
        }
    }
}
