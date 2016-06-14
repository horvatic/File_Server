using System.IO;
using FileServer.Core;
using Xunit;

namespace FileServer.Test
{
    public class FileProcessorTest
    {
        [Fact]
        public void File_Not_Found()
        {
            var fileProx = new FileProcessor();
            Assert.Equal(false, fileProx.Exists("efwefwefwefwefwefwefwefw"));
        }

        [Fact]
        public void File_Read()
        {
            var fileProx = new FileProcessor();
            Assert.NotEqual(0, fileProx.FileSize(@"C:\Program Files (x86)\Internet Explorer\ie9props.propdesc"));
        }

        [Fact]
        public void File_Not_Read()
        {
            var fileProx = new FileProcessor();
            Assert.Throws<FileNotFoundException>(() => (fileProx.FileSize("wefefwefwefwefwefwefwef")));
        }
    }
}