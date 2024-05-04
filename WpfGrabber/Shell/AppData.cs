using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfGrabber.Data;
using WpfGrabber.Readers;

namespace WpfGrabber.Shell
{
    public class AppData
    {
        public static FontData GetFont()
        {
            const string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var rd = new DataReader(Properties.Resources.AppFont) { FlipX = false };
            var fr = new FontReader(8);
            var letters = fr.ReadImages(rd, chars.Length).ToArray();
            var result = new FontData(letters, 0, chars);
            return result;
        }
    }
}
