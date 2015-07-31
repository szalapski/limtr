using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Limtr;

namespace Limtr.Tests.Integration
{
    [TestClass]
    public class LimiterTests
    {
        [TestMethod]
        public void Allows_1()
        {
            var limiter = new Limiter("free");
            for (int i = 0; i < 20; i++)
            {
                if (limiter.Allows("myOp1"))
                {
                    /* invoke the full functionality here */
                    Console.WriteLine("Executed.");
                }
                else
                {
                    Console.Error.WriteLine("Sorry, you have made too many attempts. Please try again later.");
                }
            }
        }
    }
}
