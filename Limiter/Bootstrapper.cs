using StackExchange.Redis;
using System;

namespace Limtr.Lib {
    public static class Bootstrapper { 
        private static IRedis redis;

        static Bootstrapper() {
            var local = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            var azure = new ConfigurationOptions() { EndPoints = { { "limtr.redis.cache.windows.net", 6379 } }, Password = "sAR88chKOP4xtk9dVI6uCbZTqsg5pyq/jc7eKg3pHqI=" };
            redis = new Redis(ConnectionMultiplexer.Connect(azure));
        }

        public static FatClient AzureFatClient {
            get{
                return new FatClient(new RedisLimitStore(redis));
            }
        }

        public static AdminClient AzureAdminClient {
            get {
                return new AdminClient(new RedisLimitStore(redis));
            }
        }   

    }
}
