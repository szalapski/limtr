using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Limtr
{
    public class Limiter
    {
        public Limiter(string appKey)
        {
            _appKey = appKey;
        }
        private string _appKey { get; set; }

        public Uri apiUri { get; set; } = new Uri("http://limtr.azurewebsites.net/api/");

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool Allows(string limitKey)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = apiUri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Task<HttpResponseMessage> result = client.PostAsync($"api/Limit?appKey={_appKey}&bucketName=default&operationKey={limitKey}", 
                    new ByteArrayContent(new byte[0]));
                result.Wait();
                if (result.IsFaulted || result.IsCanceled)
                {
                    throw new InvalidOperationException("failed");
                }
                else return result.ToString() == "true";

            }
        
        }

        /// <summary>
        /// Peeks at whether the operation represented by the limit key is allowed. Does not record a hit.
        /// </summary>
        /// <returns>True if the operation should be allowed, and false if the operation should be rejected or throttled.</returns>
        public bool IsAllowed(string limitKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If the operation represented by the limit key is allowed, records a hit; throws exception if the operation should be rejected or throttled.
        /// </summary>
        /// <exception cref="LimitReachedException">Thrown if the operation should be rejected or throttled</exception>
        public void Hit(string limitKey)
        {
            throw new NotImplementedException();
        }
    }
}
