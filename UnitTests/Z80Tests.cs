﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using WpfGrabber.Readers.Z80;

namespace UnitTests
{
    [TestClass]
    public class Z80Tests
    {
        [TestMethod]
        public void TestMethod1()
        {
            //var z80 = new Z80Reader(new byte[] { 1 }, 0);
            //Assert.AreEqual(z80.GetBli(5, 3), "IND");

        }

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
    }
}
