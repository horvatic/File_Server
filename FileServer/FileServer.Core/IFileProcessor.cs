namespace FileServer.Core
{
    public interface IFileProcessor
    {
        long FileSize(string path);
        bool Exists(string path);
    }
}