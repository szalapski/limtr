using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Limiter.WebService.Controllers {
    public class ValuesController : ApiController {
        // GET api/values
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // POST api/values
        public void Post([FromBody]string appKey, [FromBody]string limitKey) {

        }
    }
}
