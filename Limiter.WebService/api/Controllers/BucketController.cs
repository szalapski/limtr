using System.Web.Http;
using Limtr.Lib;

namespace Limtr.WebService.api.Controllers
{
    /// <summary>
    /// Contains web service calls for working with limiting buckets
    /// </summary>
    public class BucketController : ApiController
    {
        public BucketController() {
            _limiter = Bootstrapper.AzureAdminClient;   // todo: what about DI?
        }
        private AdminClient _limiter;

        /// <summary>
        /// Gets a bucket.
        /// </summary>
        public Bucket Get([FromUri]string appKey, [FromUri]string bucketName)
        {
            return _limiter.LoadBucket(appKey, bucketName);
        }

        /// <summary>
        /// Creates or updates the setup of a bucket.
        /// </summary>
        public void Post(Bucket bucket) {
            _limiter.Setup(bucket);
        }

    }
}