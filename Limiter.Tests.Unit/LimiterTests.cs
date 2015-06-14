using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SzLimiter.Tests.Unit {
    [TestClass]
    public class LimiterTests {
        // SUT is always Limiter in this file

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetLimitPerMinute_NotFullySetup_ThrowsException() {
            //arrange
            var sut = new Limiter("dummy");

            //act 
            int result = sut.GetLimitPerMinute();
                  
            //assert - see ExpectedExceptionAttribute above
        }

        [TestMethod]
        public void GetLimitPerMinute_NotFullySetup_ThrowsExceptionWithProperMessage() {
            //arrange
            var sut = new Limiter("dummy");

            //act 
            try {
                int result = sut.GetLimitPerMinute();

                //assume
                Assert.Fail("Did not catch any exception");
            }
            catch (Exception ex) {
                //assert - see ExpectedExceptionAttribute above
                Assert.AreEqual("Limiter does not have a backing store.", ex.Message);
            }
        }



        [TestMethod]
        public void GetLimitPerMinute_WithALimitStoreWithLimitOf42_Returns42() {
            //arrange
            var mockStore = new Mock<ILimitStore>();
            mockStore.Setup(s => s.GetLimit()).Returns(42);
            var sut = new Limiter("dummy", mockStore.Object);

            //act 
            int result = sut.GetLimitPerMinute();

            //assert - see ExpectedExceptionAttribute above
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void GetLimitPerMinute_WithALimitStoreWithLimitOfOneMillion_Returns1024() {
            //arrange
            var mockStore = new Mock<ILimitStore>();
            mockStore.Setup(s => s.GetLimit()).Returns(1000000);
            var sut = new Limiter("dummy", mockStore.Object);

            //act 
            int result = sut.GetLimitPerMinute();

            //assert - see ExpectedExceptionAttribute above
            Assert.AreEqual(1024, result);
        }
        [TestMethod]
        public void GetLimitPerMinute_WithALimitStoreWithLimitOf1025_Returns1024() {
            //arrange
            var mockStore = new Mock<ILimitStore>();
            mockStore.Setup(s => s.GetLimit()).Returns(1025);
            var sut = new Limiter("dummy", mockStore.Object);

            //act 
            int result = sut.GetLimitPerMinute();

            //assert - see ExpectedExceptionAttribute above
            Assert.AreEqual(1024, result);
        }

        [TestMethod]
        public void GetLimitPerMinute_WithALimitStoreWithLimitOf1024_Returns1024() {
            //arrange
            var mockStore = new Mock<ILimitStore>();
            mockStore.Setup(s => s.GetLimit()).Returns(1024);
            var sut = new Limiter("dummy", mockStore.Object);

            //act 
            int result = sut.GetLimitPerMinute();

            //assert - see ExpectedExceptionAttribute above
            Assert.AreEqual(1024, result);
        }

        [TestMethod]
        public void GetLimitPerMinute_WithALimitStoreWithLimitOfIntMaxValue_Returns1024() {
            //arrange
            var mockStore = new Mock<ILimitStore>();
            mockStore.Setup(s => s.GetLimit()).Returns(int.MaxValue);
            var sut = new Limiter("dummy", mockStore.Object);

            //act 
            int result = sut.GetLimitPerMinute();

            //assert - see ExpectedExceptionAttribute above
            Assert.AreEqual(1024, result);
        }



    }
}
