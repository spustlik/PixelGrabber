﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Data
{
    public class FontData
    {
        public BitBitmap[] Letters { get; }
        public int SpaceX { get; }
        private Dictionary<char, BitBitmap> font = new Dictionary<char, BitBitmap>();
        public FontData(IEnumerable<BitBitmap> letters, int spaceX, string fontCharacters)
        {
            Letters = letters.ToArray();
            SpaceX = spaceX;
            for (int i = 0; i < fontCharacters.Length; i++)
            {
                if (i < letters.Count())
                    font[fontCharacters[i]] = letters.ElementAt(i);
            }
        }

        public BitBitmap GetCharBmp(char c)
        {
            font.TryGetValue(c, out var r);
            return r;
        }
        public void DrawString(ByteBitmapRgba target, int x, int y, string text, Colorizer colorizer)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var c = GetCharBmp(text[i]);
                if (c != null)
                {
                    DrawLetter(target, x, y, c, colorizer);
                    x += c.WidthPixels;
                }
                else
                {
                    x += 6;
                }
                x += SpaceX;
            }
        }
        public void DrawString(ByteBitmap8Bit bmp, int posx, int posy, string text)
        {
            throw new NotImplementedException();
        }

        public void DrawLetter(ByteBitmapRgba target, int posx, int posy, BitBitmap chr, Colorizer colorizer = null)
        {
            if (colorizer == null)
                colorizer = Colorizers.GetColorWhite;
            target.DrawBitmap(chr, posx, posy, colorizer);
        }

        public void WriteToStream(Stream s)
        {
            var letters = font.OrderBy(pair => pair.Key).ToArray();
            var last = letters[0].Key - 1;
            for (int i = 0; i < letters.Length; i++)
            {
                var letter = letters[i];
                while (letter.Key > last + 1)
                {
                    //add empty char
                    byte[] data = GetLetterData(letter.Value);
                    Array.Clear(data, 0, data.Length);
                    s.WriteBytes(data);
                    last = letter.Key;
                }
                s.WriteBytes(GetLetterData(letter.Value));
                last = letter.Key;
            }
        }

        private static byte[] GetLetterData(BitBitmap letter)
        {
            return letter.Data;
        }

    }
}
