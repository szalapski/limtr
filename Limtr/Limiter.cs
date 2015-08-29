using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Limtr {
    /// <summary>
    /// Provides functions for rate limiting and throttling, using the Limtr web service.
    /// </summary>
    public class Limiter {
        public Limiter(string appKey = "free") {
            _appKey = appKey;
        }
        private string _appKey { get; set; }

        public Uri apiUri { get; set; } = new Uri("http://limtr.azurewebsites.net/api/");

        /// <summary>
        /// Returns whether the operation represented by the limit key should be allowed. 
        /// If so, also records a hit and throttles if necessary.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected.</returns>
        public bool Allows(string operationKey) {
            return Allowed(operationKey, doPost: true);
        }
        /// <summary>
        /// Returns whether the operation represented by the limit key should be forbidden. 
        /// If not, also records a hit and throttles if necessary.
        /// </summary>
        /// <returns>False if the operation should be allowed, and true if the operation should be rejected.</returns>
        /// <remarks>Syntactic sugar for !Allows(string).</remarks>
        public bool Forbids(string operationKey) {
            return !Allows(operationKey);
        }

        /// <summary>
        /// Peeks at whether the operation represented by the limit key should be allowed. Does not record a hit or throttle.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected.</returns>
        public bool IsAllowed(string operationKey) {
            return Allowed(operationKey, doPost: false);
        }
        /// <summary>
        /// Peeks at whether the operation represented by the limit key should be forbidden. Does not record a hit or throttle.
        /// </summary>
        /// <returns>False if the operation should be allowed, and true if the operation should be rejected.</returns>
        public bool IsForbidden(string operationKey) {
            return !IsAllowed(operationKey);
        }

        /// <summary>
        /// If the operation represented by the limit key should allowed, records a hit and throttles if necessary; 
        /// throws exception if the operation should be rejected.
        /// </summary>
        /// <exception cref="OverLimitException">Thrown if the operation should be rejected or throttled.</exception> 
        /// <remarks>Use only when you expect to be under the limit. Otherwise, use Allowed(string).</remarks>
        public void Hit(string operationKey) {
            if (!Allowed(operationKey, doPost: true)) throw new OverLimitException();
        }

        private bool Allowed(string operationKey, bool doPost = false) {
            using (var client = new HttpClient()) {
                client.BaseAddress = apiUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string limitQueryString = $"Limit?appKey={_appKey}&bucketName=default&operationKey={operationKey}";
                HttpResponseMessage response = doPost
                    ? client.PostAsync(limitQueryString, new ByteArrayContent(new byte[0])).Result
                    : client.GetAsync(limitQueryString).Result;
                if (response.IsSuccessStatusCode) {
                    return response.Content.ReadAsStringAsync().Result == "true";
                }
                else {
                    throw new InvalidOperationException(); // TODO: better error handling
                }
            }
        }

    }
}
