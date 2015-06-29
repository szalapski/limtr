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
        /// Returns whether the operation represented by a operation key in a bucket is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed; false if the operation should be rejected or throttled.</returns>
        public bool Get([FromUri]string appKey, [FromUri]string bucketName, [FromUri]string operationKey) {
            return _limiter.IsAllowed(appKey, bucketName, operationKey);
        }

        /// <summary>
        /// Returns whether the operation represented by a operation key in a bucket is allowed.  Records a hit if it is.
        /// </summary>
        /// <returns>True if the operation should be allowed; false if the operation should be rejected or throttled.</returns>
        public bool Post([FromUri]string appKey, [FromUri]string bucketName, [FromUri]string operationKey) {
            return _limiter.Allows(appKey, bucketName, operationKey);
        }
    }
}
