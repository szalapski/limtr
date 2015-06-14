using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SzLimiter.Tests.Unit {
    [TestClass]
    public class LimiterTests {

        [TestMethod]
        void Limit1() {
            var mockStore = new Mock<ILimitStore>();
            var limiter = new Limiter("LimiterTests", mockStore.Object);
            if (limiter.Allows("foo")) {

            }

        }
     
    }
}
