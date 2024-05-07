namespace WpfGrabber.Readers
{
    public class ReaderImageResult
    {
        public ByteBitmap8Bit bitmap;
        public int position;
        public int end;

        public ReaderImageResult(ByteBitmap8Bit bitmap, int position, int end)
        {
            this.bitmap = bitmap;
            this.position = position;
            this.end = end;
        }
    }
}