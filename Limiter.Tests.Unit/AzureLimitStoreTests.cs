﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
<<<<<<< HEAD
using StackExchange.Redis;
using Moq;
=======
>>>>>>> 9e74e6d461be1778fb0bf1cf2c8702b07dd7a51f

namespace Limiter.Tests.Unit {
    [TestClass]
    public class AzureLimitStoreTests {
<<<<<<< HEAD

        
=======
        private const string appKey = "unitTestsAppKey";

        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallOnce_DoesNotLimit() {
            var store = new RedisLimitStore(2);
            bool result = store.Limit(appKey, "1.2.3.1");
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerMinuteCallThrice_Limits() {
            var limiter = new RedisLimitStore(2);
            string testLimitKey = "1.2.3.2";
            limiter.Limit(appKey, testLimitKey);
            limiter.Limit(appKey, testLimitKey);

            bool result = limiter.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallOnce_DoesNotLimit() {
            var store = new RedisLimitStore(2, TimeSpan.FromSeconds(30));

            bool result = store.Limit(appKey, "1.2.3.3");

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void Limit_DefaultBucketFor2PerHalfMinuteCallThrice_Limits() {
            var limiter = new RedisLimitStore(2, TimeSpan.FromSeconds(30));
            string testLimitKey = "1.2.3.4";
            limiter.Limit(appKey, testLimitKey);
            limiter.Limit(appKey, testLimitKey);
            bool result = limiter.Limit(appKey, testLimitKey);

            Assert.IsTrue(result);
        }
>>>>>>> 9e74e6d461be1778fb0bf1cf2c8702b07dd7a51f
    }
}
