using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Limtr.Lib;

namespace Limtr.WebService.Controllers {
    public class LimitController : ApiController {
        public LimitController() {
            _limiter = Bootstrapper.AzureFatClient;   // todo: what about app key?
        }

        private FatClient _limiter;

        /// <summary>
        /// Peeks at whether the operation represented by the limit key is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Get([FromUri]string appKey, [FromUri]string limitKey) {
            // TODO: check for valid appKey

            return _limiter.IsAllowed(limitKey);
        }

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Post([FromUri]string appKey, [FromUri]string limitKey) {
            // TODO: check for valid appKey

            return _limiter.Allows(limitKey);
        }
    }
}
