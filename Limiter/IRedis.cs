using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;

namespace Limtr.Lib {
    public interface IRedis {
        IDatabase Database { get; }
        EndPoint[] Endpoints { get; }
        IEnumerable<IServer> Servers { get; }
    }
}
