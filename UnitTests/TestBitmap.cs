using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfGrabber;

namespace UnitTests
{
    [TestClass]
    public class TestBitmap
    {
        [TestMethod]
        public void TestBitBitmap()
        {
            {
                var bmp = new BitBitmap(4, 2);
                Assert.AreEqual(4, bmp.WidthPixels);
                Assert.AreEqual(1, bmp.WidthBytes);
                Assert.AreEqual(2, bmp.Height);
            }
            {
                var bmp = new BitBitmap(7, 2);
                Assert.AreEqual(1, bmp.WidthBytes);
            }
            {
                var bmp = new BitBitmap(8, 2);
                Assert.AreEqual(1, bmp.WidthBytes);
            }
            {
                var bmp = new BitBitmap(8, 2);
                Assert.AreEqual(2, bmp.Data.Length);
                Assert.AreEqual(0, bmp.Data[0]);
                Assert.AreEqual(0, bmp.Data[1]);

                bmp.SetPixel(0, 0, true);
                Assert.AreEqual(1, bmp.Data[0]);
                Assert.AreEqual(0, bmp.Data[1]);

                bmp.SetPixel(1, 0, true);
                Assert.AreEqual(0b00000011, bmp.Data[0]);
                Assert.AreEqual(0, bmp.Data[1]);

                bmp.SetPixel(7, 0, true);
                bmp.SetPixel(1, 0, false);
                Assert.AreEqual(0b10000001, bmp.Data[0]);
                Assert.AreEqual(0, bmp.Data[1]);


                bmp.SetPixel(3, 1, true);
                bmp.SetPixel(4, 1, false);
                Assert.AreEqual(0b00001000, bmp.Data[1]);

            }

        }
    }
}
