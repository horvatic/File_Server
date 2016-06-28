using System;
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
        public void Read_Files_Bytes()
        {
            var guid = Guid.NewGuid();
            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var stream = File.Open("c:/" + guid + ".tmp",
                FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            stream.Write(data, 0, 2);
            stream.Close();

            var fileProx = new FileProcessor();
            var reader = fileProx.GetFileStream("c:/" + guid + ".tmp");
            Assert.NotNull(reader);
            reader.Close();
            File.Delete("c:/" + guid + ".tmp");
        }

        [Fact]
        public void File_Not_Read()
        {
            var fileProx = new FileProcessor();
            Assert.Throws<FileNotFoundException>(() => (fileProx.FileSize("wefefwefwefwefwefwefwef")));
        }
    }
}