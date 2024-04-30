﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WpfGrabber.Converters;

namespace UnitTests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            {
                var s = EnumItemsConverter.AddSpacesToUpperCase("KarelJedeDomu");
                Assert.AreEqual("Karel Jede Domu", s);
            }
            {
                var s = EnumItemsConverter.AddSpacesToUpperCase("nictamneni");
                Assert.AreEqual("nictamneni", s);
            }
            {
                var s = EnumItemsConverter.AddSpacesToUpperCase("ABCDEF");
                Assert.AreEqual("ABCDEF", s);
            }
        }
    }
}
