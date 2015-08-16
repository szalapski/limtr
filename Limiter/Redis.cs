using System.Collections.Generic;
using System.Net;
using StackExchange.Redis;

namespace Limtr.Lib {
    public class Redis : IRedis {
        public Redis(ConnectionMultiplexer muxer) {
            _muxer = muxer;
            Endpoints = muxer.GetEndPoints();
        }  
        private ConnectionMultiplexer _muxer;

        public IDatabase Database {
            get {
                return _muxer.GetDatabase();
            }
        }
        public EndPoint[] Endpoints { get; private set; }
        public IEnumerable<IServer> Servers {
            get {
                foreach (EndPoint endpoint in Endpoints)
                    yield return _muxer.GetServer(endpoint);
            }
        }



    }
}
