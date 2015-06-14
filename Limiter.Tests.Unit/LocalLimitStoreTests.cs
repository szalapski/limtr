using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SzLimiter.Tests.Unit {
    [TestClass]
    public class LocalLimitStoreTests {
        private const string appKey = "LocalLimitStoreTests";

        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var store = new LocalLimitStore(2);
            bool result = store.Limit(appKey, Guid.NewGuid().ToString());
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var limiter = new LocalLimitStore(2);
            string testLimitKey = Guid.NewGuid().ToString();
            limiter.Limit(appKey, testLimitKey);
            limiter.Limit(appKey, testLimitKey);

            bool result = limiter.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallOnce_DoesNotLimit() {
            var store = new LocalLimitStore(2, TimeSpan.FromSeconds(30));

            bool result = store.Limit(appKey, Guid.NewGuid().ToString());

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallThrice_Limits() {
            var limiter = new LocalLimitStore(2, TimeSpan.FromSeconds(30));
            string testLimitKey = Guid.NewGuid().ToString();
            limiter.Limit(appKey, testLimitKey);
            limiter.Limit(appKey, testLimitKey);
            bool result = limiter.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }
    }
}
