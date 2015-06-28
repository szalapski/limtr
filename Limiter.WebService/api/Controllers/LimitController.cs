using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Limtr.Lib;

namespace Limtr.WebService.Controllers {

    /// <summary>
    /// Contains web service calls for working with limiting
    /// </summary>
    public class LimitController : ApiController {
        public LimitController() {
            _limiter = Bootstrapper.AzureFatClient;   // todo: what about DI?
        }

        private FatClient _limiter;

        /// <summary>
        /// Returns whether the operation represented by a limit key is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed; false if the operation should be rejected or throttled.</returns>
        public bool Get([FromUri]string appKey, [FromUri]string limitKey) {
            // TODO: check for valid appKey

            return _limiter.IsAllowed(appKey, limitKey);
        }

        /// <summary>
        /// Returns whether the operation represented by a limit key is allowed.  Records a hit if it is.
        /// </summary>
        /// <returns>True if the operation should be allowed; false if the operation should be rejected or throttled.</returns>
        public bool Post([FromUri]string appKey, [FromUri]string limitKey) {
            // TODO: check for valid appKey

            return _limiter.Allows(appKey, limitKey);
        }
    }
}
