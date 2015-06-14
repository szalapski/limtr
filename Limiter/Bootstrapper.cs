using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SzLimiter {
    public static class Bootstrapper { 
        private static ConnectionMultiplexer muxer;

        static Bootstrapper() {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            muxer = ConnectionMultiplexer.Connect(redisOptions);
        }

        public static Limiter AzureLimiter {
            get{
                return new Limiter("test", new RedisLimitStore(muxer.GetDatabase(), 2));
            }
        }   
    }
}
