namespace Limtr.Lib {
    public interface ILimitStore {
        bool Allows(string _appKey, string limitKey);
        bool IsAllowed(string _appKey, string limitKey);
    }
}
