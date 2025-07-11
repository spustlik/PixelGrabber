﻿using System;
using WpfGrabber.Data;

namespace WpfGrabber
{
    public class ByteBitmapRgba
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Data { get; private set; }
        public ByteBitmapRgba(int width, int height, uint[] data = null)
        {
            this.Width = width;
            this.Height = height;
            Data = data ?? new uint[width * height];
        }


        public byte[] ToBytes()
        {
            var bytes = new byte[Data.Length * sizeof(uint)];
            Array.Copy(Data, bytes, Data.Length);
            return bytes;
        }

        public void SetPixel(int x, int y, uint value)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            Data[x + y * Width] = value;
        }
        public uint GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return 0x00000000;
            return Data[x + y * Width];
        }

    }
}