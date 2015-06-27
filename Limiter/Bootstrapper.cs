using StackExchange.Redis;
using System;

namespace Limtr.Lib {
    public static class Bootstrapper { 
        private static ConnectionMultiplexer muxer;

        static Bootstrapper() {
            var local = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            var azure = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            muxer = ConnectionMultiplexer.Connect(azure);
        }

        public static FatClient AzureFatClient {
            get{
                return new FatClient(new RedisLimitStore(muxer.GetDatabase()));
            }
        }   
    }
}
