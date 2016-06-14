using FileServer.Core;
using Moq;

namespace FileServer.Test
{
    public class MockDirectoryProcessor : IDirectoryProcessor
    {
        private readonly Mock<IDirectoryProcessor> _mock;

        public MockDirectoryProcessor()
        {
            _mock = new Mock<IDirectoryProcessor>();
        }

        public bool Exists(string path)
        {
            return _mock.Object.Exists(path);
        }

        public string[] GetDirectories(string path)
        {
            return _mock.Object.GetDirectories(path);
        }

        public string[] GetFiles(string path)
        {
            return _mock.Object.GetFiles(path);
        }

        public MockDirectoryProcessor StubGetFiles(string[] files)
        {
            _mock.Setup(m => m.GetFiles(It.IsAny<string>())).Returns(files);
            return this;
        }

        public MockDirectoryProcessor StubGetDirectories(string[] dirs)
        {
            _mock.Setup(m => m.GetDirectories(It.IsAny<string>())).Returns(dirs);
            return this;
        }

        public MockDirectoryProcessor StubExists(bool isDir)
        {
            _mock.Setup(m => m.Exists(It.IsAny<string>())).Returns(isDir);
            return this;
        }

        public void VerifyExists(string path)
        {
            _mock.Verify(m => m.Exists(path), Times.AtLeastOnce);
        }

        public void VerifyGetFiles(string path)
        {
            _mock.Verify(m => m.GetFiles(path), Times.AtLeastOnce);
        }

        public void VerifyGetDirectories(string path)
        {
            _mock.Verify(m => m.GetDirectories(path), Times.AtLeastOnce);
        }
    }
}