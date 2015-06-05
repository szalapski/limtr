using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Limiter.Tests.Unit {
    [TestClass]
    public class LocalLimitStoreTests {
        private const string appKey = "unitTestsAppKey";

        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var store = new LocalLimitStore(2);
            bool result = store.Limit(appKey, "1.2.3.4");
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var limiter = new LocalLimitStore(2);
            string testLimitKey = "1.2.3.4";
            limiter.Limit(appKey, testLimitKey);
            limiter.Limit(appKey, testLimitKey);
            bool result = limiter.Limit(appKey, testLimitKey);
            Assert.IsTrue(result);
        }
    }
}
