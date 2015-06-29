using System;
namespace Limtr.Lib {
    /// <summary>
    /// Provides limiting functionality.
    /// </summary>
    public class FatClient {
        public FatClient(ILimitStore store) {
            _store = store;
        }
        private ILimitStore _store { get; set; }
        private string _appKey {get; set;}

        /// <summary>
        /// If the operation represented by a key in the default bucket is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string appKey, string operationKey) {
            return Allows(appKey, "default", operationKey);
        }
        /// <summary>
        /// If the operation represented by a key in a bucket is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string appKey, string bucket, string operationKey) {
            // TODO: need a call that lets us get info on whether appkey or bucket is not set up?
            VerifyBucketExists(appKey, bucket);
            return _store.Allows(appKey, bucket, operationKey);
        }

        /// <summary>
        /// Peeks at whether the operation represented by a key in the default bucket is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool IsAllowed(string appKey, string operationKey) {
            return IsAllowed(appKey, "default", operationKey);
        }

        /// <summary>
        /// Peeks at whether the operation represented by a key in a bucket is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool IsAllowed(string appKey, string bucketName, string operationKey) {
            VerifyBucketExists(appKey, bucketName);
            return _store.IsAllowed(appKey, "default", operationKey);
        }

        /// <summary>
        /// If the operation represented by a key in the default bucket is allowed, records a hit; throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string appKey, string operationKey) {
            Hit(appKey, "default", operationKey);
        }

        /// <summary>
        /// If the operation represented by a limit key in a bucket is allowed, records a hit; throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string appKey, string bucketName, string operationKey) {
            if (!Allows(appKey, bucketName, operationKey)) throw new LimitReachedException();
        }

        private void VerifyBucketExists(string appKey, string bucketName){
            _store.LoadBucket(appKey, bucketName);
        }


    }
}
