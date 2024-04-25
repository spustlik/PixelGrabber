using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using WpfGrabber;
using WpfGrabber.Readers;

namespace UnitTests
{
    [TestClass]
    public class FontTests
    {
        [TestMethod]
        public void TestFlip()
        {
            { 
                var r = BitReader.GetFlippedX(0b11110000);
                Assert.AreEqual(r, 0b00001111);
            }
            {
                var r = BitReader.GetFlippedX(0b10110000);
                Assert.AreEqual(r, 0b00001101);
            }
            {
                var r = BitReader.GetFlippedX(0b00001111);
                Assert.AreEqual(r, 0b11110000);
            }
            {
                var r = BitReader.GetFlippedX(0b00000001);
                Assert.AreEqual(r, 0b10000000);
            }
        }

        [TestMethod]
        public void GenerateBinaryNoise()
        {
            var sb = new StringBuilder();
            var rnd= new Random();
            for (int i = 0; i < 10000; i++)
            {
                sb.Append( rnd.Next(2)==0 ? "1" : "0" );
            }
            var s = sb.ToString();
        }
    }
}
