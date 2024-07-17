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
        [TestMethod]
        public void TestFromString()
        {
            Assert.AreEqual("R=FF,G=FF,B=FF,A=FF", ByteColor.FromString("#FFF").ToString("RGBA"));
            Assert.AreEqual("R=11,G=22,B=33,A=FF", ByteColor.FromString("#123").ToString("RGBA"));
            Assert.AreEqual("R=AA,G=BB,B=CC,A=DD", ByteColor.FromString("#ABCD").ToString("RGBA"));
            Assert.AreEqual("R=12,G=34,B=56,A=FF", ByteColor.FromString("#123456").ToString("RGBA"));
            Assert.AreEqual("R=78,G=AB,B=CD,A=80", ByteColor.FromString("78ABCD80").ToString("RGBA"));

            Assert.AreEqual("R=01,G=02,B=03,A=FF", ByteColor.FromString("1,2,3").ToString("RGBA"));
            Assert.AreEqual("R=01,G=02,B=03,A=04", ByteColor.FromString("1,2,3,4").ToString("RGBA"));
        }
    }
}
