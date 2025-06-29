using System.Collections.Generic;

namespace WpfGrabber.Readers
{
    public class ReaderImageResult
    {
        public ByteBitmap8Bit Bitmap { get; }
        public int Position { get; }
        public int End { get; }
        public string Description { get; set; }
        public string Overlay { get; set; }
        public ReaderImageResult(ByteBitmap8Bit bitmap, int position, int end)
        {
            this.Bitmap = bitmap;
            this.Position = position;
            this.End = end;
        }
    }

    public abstract class EngineReader
    {
        protected DataReader Reader { get; }

        protected EngineReader(DataReader reader)
        {
            this.Reader = reader;
        }
        public abstract IEnumerable<ReaderImageResult> ReadImages();
    }
}