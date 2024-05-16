using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfGrabber.Data;

namespace UnitTests
{
    [TestClass]
    public class ColorTests
    {
        [TestMethod]
        public void TestFromUInt()
        {
            var c = ByteColor.FromUint(0x78123456);
            Assert.AreEqual(0x78, c.A);
            Assert.AreEqual(0x12, c.R);
            Assert.AreEqual(0x34, c.G);
            Assert.AreEqual(0x56, c.B);
        }
    }
}
