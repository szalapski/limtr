﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SzLimiter;

namespace SzLimiter.WebService.Controllers {
    public class ValuesController : ApiController {
        public ValuesController() {
            _limiter = Bootstrapper.AzureLimiter;   // todo: what about app key?
        }

        private Limiter _limiter;

        // GET api/values
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }

        // POST api/values
        public bool Post([FromBody]string appKey, [FromBody]string limitKey) {
            return _limiter.Allows(limitKey);
        }
    }
}