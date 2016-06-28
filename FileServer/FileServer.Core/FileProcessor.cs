using System.IO;

namespace FileServer.Core
{
    public class FileProcessor : IFileProcessor
    {
        public long FileSize(string path)
        {
            return (new FileInfo(path)).Length;
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public FileStream GetFileStream(string path)
        {
            return File.Open(path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
        }
    }
}