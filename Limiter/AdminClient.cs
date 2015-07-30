namespace Limtr.Lib {
    public class AdminClient {
        public AdminClient(ILimitStore store) {
            _store = store;
        }

        private ILimitStore _store { get; set; }

        public void Setup(Bucket bucket) => _store.Setup(bucket);
        public Bucket LoadBucket(string appKey, string name) => _store.LoadBucket(appKey, name);
        
    }
}
