using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGrabber.Data
{
    public class FontData
    {
        public ByteBitmap8Bit[] Letters { get; }
        public int SpaceX { get; }
        private Dictionary<char, ByteBitmap8Bit> font = new Dictionary<char, ByteBitmap8Bit>();
        public FontData(IEnumerable<ByteBitmap8Bit> letters, int spaceX, string fontCharacters)
        {
            Letters = letters.ToArray();
            SpaceX = spaceX;
            for (int i = 0; i < fontCharacters.Length; i++)
            {
                if (i < letters.Count())
                    font[fontCharacters[i]] = letters.ElementAt(i);
            }
        }

        public ByteBitmap8Bit GetCharBmp(char c)
        {
            font.TryGetValue(c, out var r);
            return r;
        }
        public void DrawString(ByteBitmapRgba target, int x, int y, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var c = GetCharBmp(text[i]);
                if (c != null)
                {
                    DrawLetter(target, x, y, c);
                    x += c.Width;
                }
                else
                {
                    x += 6;
                }
                x += SpaceX;
            }
        }

        public void DrawLetter(ByteBitmapRgba target, int posx, int posy, ByteBitmap8Bit c, uint color = 0xFF000000)
        {
            for (var y = 0; y < c.Height; y++)
            {
                for (var x = 0; x < c.Width; x++)
                {
                    if (c.GetPixel(x, y) != 0)
                        target.SetPixel(posx + x, posy + y, 0xFF000000 | color);
                }
            }
        }
    }
}
