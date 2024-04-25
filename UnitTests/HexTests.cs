using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfGrabber.Readers;

namespace UnitTests
{
    [TestClass]
    public class HexTests
    {
        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("0001", HexReader.ToHex(1));
            Assert.AreEqual("01", HexReader.ToHex(1,2));
            Assert.AreEqual("1234", HexReader.ToHex(0x1234));
            Assert.AreEqual("FEDC", HexReader.ToHex(0xFEDC));
            Assert.AreEqual("A5", HexReader.ToHex(0xA5,2));

        }
    }
}
