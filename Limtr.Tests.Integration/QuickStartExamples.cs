using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Reflection.MethodBase;
using Limtr;  // others will need this


namespace Limtr.Tests.Integration {
    [TestClass]
    public class QuickStartExamples {

        [TestMethod]
        public bool AttemptLogin(string userName) {
            if (new Limiter().Forbids(userName)) return false;
            /* invoke the full functionality here */
            return true;
        }
    }
}
