using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Limtr.Lib;

namespace Limtr.Lib.Tests.Unit {
    [TestClass]
    public class FatClientTests {

        [TestMethod]
        void Limit1() {
            var mockStore = new Mock<ILimitStore>();
            var limiter = new Limtr.Lib.FatClient("LimiterTests", mockStore.Object);
            if (limiter.Allows("foo")) {

            }

        }
     
    }
}
