using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using WpfGrabber.Readers.Z80;
using WpfGrabber.ViewParts;

namespace UnitTests
{
    [TestClass]
    public class Z80Tests
    {

        [TestMethod]
        public void TestBatman()
        {
            var bytes = File.ReadAllBytes(@"E:\GameWork\8bitgames\sord\DISK64\BATMAN.COM");
            var z80 = new Z80Reader(bytes, 0);
            var sb = new StringBuilder();
            while (z80.Addr<z80.Data.Length)
            {
                try
                {
                    var instr = z80.ReadInstruction();
                    sb.Append(instr.Start.ToString("X4")).Append(":\t\t");
                    sb.AppendLine(instr.ToString());
                }
                catch (Exception e)
                {
                    sb.AppendLine(">>" + e.Message);
                    z80.ReadByte();//skip byte
                }
            }
            File.WriteAllText(@"batman.dmp", sb.ToString());

        }
        [TestMethod]
        public void TestBatmanInstr()
        {
            var bytes = File.ReadAllBytes(@"E:\GameWork\8bitgames\sord\DISK64\BATMAN.COM");
            var z80 = new Z80Reader(bytes, 0x28d);
            var sb = new StringBuilder();
            var instr = z80.ReadInstruction();
            sb.Append(instr.Start.ToString("X4")).Append(":\t\t");
            sb.AppendLine(instr.ToString());
        }

        [TestMethod]
        public void TestAddrParser()
        {
            var r = Z80DumpViewPart.CreateLineInlines(@"1234: 01 02 03      LD A, 0x1234   JP L5678").ToArray();
            Assert.AreEqual(4, r.Length);
            var r0 = r[0] as Run;
            Assert.IsNotNull(r0);
            Assert.AreEqual(r0.Text, "1234: 01 02 03      LD A, ");

            var h1 = r[1] as Hyperlink;
            Assert.IsNotNull(h1);
            Assert.AreEqual(HyperLinkText(h1), "0x1234");

            var r2 = r[2] as Run;
            Assert.IsNotNull(r2);
            Assert.AreEqual(r2.Text, "   JP ");

            var h3 = r[3] as Hyperlink;
            Assert.IsNotNull(h3);
            Assert.AreEqual(HyperLinkText(h3), "L5678");
        }

        private string HyperLinkText(Hyperlink h)
        {
            var r = h.Inlines.ToArray().First() as Run;
            return r.Text;
        }
    }
}
