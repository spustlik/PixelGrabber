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
            var c = ColorHelper.ColorFromUint(0x78123456);
            Assert.AreEqual(0x78, c.a);
            Assert.AreEqual(0x12, c.r);
            Assert.AreEqual(0x34, c.g);
            Assert.AreEqual(0x56, c.b);
        }
    }
}
