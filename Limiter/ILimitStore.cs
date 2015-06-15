using System;
namespace Limtr.Lib {
    public interface ILimitStore {
        bool Allows(string appKey, string bucket, string limitKey);
        bool IsAllowed(string appKey, string bucket, string limitKey);
        void Setup(string appKey, string bucket, long limit, TimeSpan limitInterval);
    }
}