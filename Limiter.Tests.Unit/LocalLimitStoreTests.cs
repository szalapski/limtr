using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SzLimiter.Tests.Unit {
    [TestClass]
    public class LocalLimitStoreTests {
        private const string appKey = "LocalLimitStoreTests";

        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var store = new LocalLimitStore(2);
            bool result = store.Allows(appKey, Guid.NewGuid().ToString());
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var limiter = new LocalLimitStore(2);
            string testLimitKey = Guid.NewGuid().ToString();
            limiter.Allows(appKey, testLimitKey);
            limiter.Allows(appKey, testLimitKey);

            bool result = limiter.Allows(appKey, testLimitKey);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Allows_DefaultBucketFor2PerHalfMinuteCallOnce_DoesNotLimit() {
            var store = new LocalLimitStore(2, TimeSpan.FromSeconds(30));

            bool result = store.Allows(appKey, Guid.NewGuid().ToString());

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void Allows_DefaultBucketFor2PerHalfMinuteCallThrice_Limits() {
            var limiter = new LocalLimitStore(2, TimeSpan.FromSeconds(30));
            string testLimitKey = Guid.NewGuid().ToString();
            limiter.Allows(appKey, testLimitKey);
            limiter.Allows(appKey, testLimitKey);
            bool result = limiter.Allows(appKey, testLimitKey);

            Assert.IsFalse(result);
        }
    }
}
