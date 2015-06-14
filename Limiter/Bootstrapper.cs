using StackExchange.Redis;

namespace Limtr.Lib {
    public static class Bootstrapper { 
        private static ConnectionMultiplexer muxer;

        static Bootstrapper() {
            var redisOptions = new ConfigurationOptions() { EndPoints = { { "localhost", 6379 } }, AbortOnConnectFail = false };
            muxer = ConnectionMultiplexer.Connect(redisOptions);
        }

        public static FatClient AzureFatClient {
            get{
                return new FatClient("test", new RedisLimitStore(muxer.GetDatabase(), 2));
            }
        }   
    }
}
