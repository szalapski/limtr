using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Reflection.MethodBase;

namespace Limtr.Tests.Integration {
    [TestClass]
    public class LimiterTests {
        [TestMethod]
        public void Allows_20ExecutionsOnFreeApp_AllowsFirst10Only() {
            var limiter = new Limiter("free");
            for (int i = 0; i < 20; i++) {
                if (limiter.Allows(GetCurrentMethod().Name)) {
                    /* invoke the full functionality here */
                    Console.WriteLine($"Executed at {DateTime.Now.ToString("o")}.");
                    Assert.IsTrue(i <= 9, $"Iteration index {i} was improperly allowed");
                }
                else {
                    Console.Error.WriteLine($"{DateTime.Now.ToString("o")} Sorry, you have made too many attempts. Please try again later.");
                    Assert.IsTrue(i >= 10, $"Iteration index {i} was improperly disallowed");
                }
            }
        }

        [TestMethod]
        public void Allows_10ExecutionsOnFreeApp_ThrottlesLast3AtLeast200ms() {
            var limiter = new Limiter("free");
          
            for (int i = 0; i < 10; i++) {
                Stopwatch sw = Stopwatch.StartNew();
                Assert.IsTrue(limiter.Allows(GetCurrentMethod().Name), "Limiter improperly refused the operation");
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
                if (i >= 7) Assert.IsTrue(sw.ElapsedMilliseconds > 200);
            }
        }


        [TestMethod]
        public void IsAllowed_20ExecutionsOnFreeApp_AllowsAll() {
            var limiter = new Limiter("free");
            for (int i = 0; i < 20; i++) {
                Assert.IsTrue(limiter.IsAllowed(GetCurrentMethod().Name), $"Iteration index {i} was improperly disallowed");
            }
        }
    }
}
