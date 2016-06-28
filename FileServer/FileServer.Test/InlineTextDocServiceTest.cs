using System;
using System.IO;
using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class InlineTextDocServiceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new InlineTextDocService());
        }

        [Theory]
        [InlineData("GET /hello.txt HTTP/1.1")]
        [InlineData("GET /hi.txt HTTP/1.0")]
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
            var inlineTextDocService = new InlineTextDocService();

            Assert.True(inlineTextDocService
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
            var inlineTextDocService = new InlineTextDocService();

            Assert.False(inlineTextDocService
                .CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();
            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var writeStream = File.Open("c:/" + guid + ".txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.Read);
            writeStream.Write(data, 0, 2);
            writeStream.Close();

            var readStream = File.Open("c:/" + guid + ".txt",
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
            var inlineTextDocService = 
                new InlineTextDocService();

            var statusCode =
                inlineTextDocService
                    .ProcessRequest("GET /" + guid + ".txt HTTP/1.1",
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
                                       + "; filename = " + guid + ".txt\r\n"),
                GetByteCount("Content-Disposition: inline"
                             + "; filename = " + guid + ".txt\r\n"));
            zSocket.VerifySend(GetByte("Content-Type: text/plain\r\n"),
                GetByteCount("Content-Type: text/plain\r\n"));
        }

        [Fact]
        public void Send_Data_Error()
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
            var inlineTextDocService =
                new InlineTextDocService();

            var statusCode =
                inlineTextDocService
                    .ProcessRequest("GET /" + guid + ".txt HTTP/1.1",
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