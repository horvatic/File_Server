namespace FileServer.Core
{
    public interface IDirectoryProcessor
    {
        bool Exists(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
    }
}