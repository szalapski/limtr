using StackExchange.Redis;
using System;

namespace Limtr.Lib {
    public static class Bootstrapper { 
        private static ConnectionMultiplexer muxer;

        static Bootstrapper() {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            muxer = ConnectionMultiplexer.Connect(redisOptions);
        }

        public static FatClient AzureFatClient {
            get{
                return new FatClient(new RedisLimitStore(muxer.GetDatabase()));
            }
        }   
    }
}
