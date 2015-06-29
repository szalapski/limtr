using System;
namespace Limtr.Lib {
    public interface ILimitStore {
        bool Allows(string appKey, string bucket, string limitKey);
        bool IsAllowed(string appKey, string bucket, string limitKey);
        void Setup(Bucket bucket);
        bool IsActiveAppKey(string appKey);
        bool IsActiveBucket(string appKey, string bucket);
    }
}