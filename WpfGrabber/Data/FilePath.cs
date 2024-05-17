using System;
using System.Collections.Generic;
using System.IO;

namespace WpfGrabber.Data
{
    public class FilePath
    {
        public string Name => Path.GetFileName(FullPath);
        public string Directory => Path.GetDirectoryName(FullPath);
        public string Extension => Path.GetExtension(FullPath);
        public string FullPath { get; }
        public override string ToString() => FullPath;
        public FilePath ChangeExtension(string ext) => new FilePath(Path.ChangeExtension(FullPath, ext));

        public FilePath(string path)
        {
            FullPath = path;
        }
        public static FilePath FromPath(string path, string name) => new FilePath(Path.Combine(path, name));

    }
}

