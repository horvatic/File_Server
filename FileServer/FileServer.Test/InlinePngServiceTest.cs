using System;
using System.IO;
using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class InlinePngServiceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new InlinePngService());
        }

        [Theory]
        [InlineData("GET /hello.png HTTP/1.1")]
        [InlineData("GET /hi.png HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePngService = new InlinePngService();

            Assert.True(inlinePngService
                .CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /hello.gr HTTP/1.1")]
        [InlineData("GET /hi.er HTTP/1.0")]
        public void Cant_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePngService = new InlinePngService();

            Assert.False(inlinePngService.CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();
            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var writeStream = File.Open("c:/" + guid + ".png",
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.Read);
            writeStream.Write(data, 0, 2);
            writeStream.Close();

            var readStream = File.Open("c:/" + guid + ".png",
                FileMode.Open, FileAccess.Read,
                FileShare.Read);
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true)
                .StubGetFileStream(readStream)
                .StubFileSize(2);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePngService = new InlinePngService();

            var statusCode = inlinePngService
                .ProcessRequest("GET /" + guid + ".png HTTP/1.1",
                    new HttpResponse(zSocket),
                    properties);
            File.Delete("c:/" + guid + ".png");
            Assert.Equal("200 OK", statusCode);
            zSocket.VerifySend(data, 2);
            zSocket.VerifySend(GetByte("HTTP/1.1 200 OK\r\n"),
                GetByteCount("HTTP/1.1 200 OK\r\n"));
            zSocket.VerifySend(GetByte("Content-Length: 2\r\n\r\n"),
                GetByteCount("Content-Length: 2\r\n\r\n"));
            zSocket.VerifySend(GetByte("Cache-Control: no-cache\r\n"),
                GetByteCount("Cache-Control: no-cache\r\n"));
            zSocket.VerifySend(GetByte("Content-Disposition: inline"
                                       + "; filename = " + guid + ".png\r\n"),
                GetByteCount("Content-Disposition: inline"
                             + "; filename = " + guid + ".png\r\n"));
            zSocket.VerifySend(GetByte("Content-Type: image/png\r\n"),
                GetByteCount("Content-Type: image/png\r\n"));
        }
        [Fact]
        public void Send_Error_Data()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();
            
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true)
                .StubGetFileStream(null)
                .StubFileSize(2);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePngService = new InlinePngService();

            var statusCode = inlinePngService
                .ProcessRequest("GET /" + guid + ".png HTTP/1.1",
                    new HttpResponse(zSocket),
                    properties);
            Assert.Equal("200 OK", statusCode);
            
        }
        private int GetByteCount(string message)
        {
            return Encoding.ASCII.GetByteCount(message);
        }

        private byte[] GetByte(string message)
        {
            return Encoding.ASCII.GetBytes(message);
        }
    }
}