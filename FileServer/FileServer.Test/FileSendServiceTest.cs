using System;
using System.IO;
using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class FileSendServiceTest
    {
        [Fact]
        public void Make_Class_Not_Null()
        {
            Assert.NotNull(new FileSendService());
        }

        [Theory]
        [InlineData("GET /hello.exe HTTP/1.1")]
        [InlineData("GET /hi.go HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var fileSendService = new FileSendService();

            Assert.True(fileSendService.CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /hello.txt HTTP/1.1")]
        [InlineData("GET /hi.pdf HTTP/1.0")]
        [InlineData("GET /hi.png HTTP/1.0")]
        [InlineData("POST /hi.png HTTP/1.0")]
        public void Cant_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var fileSendService = new FileSendService();

            Assert.False(fileSendService.CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /hello.txt HTTP/1.1")]
        [InlineData("GET /hi.pdf HTTP/1.0")]
        [InlineData("GET /hi.png HTTP/1.0")]
        [InlineData("POST /hi.png HTTP/1.0")]
        public void Cant_Process_Null_Path(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(null,
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var fileSendService = new FileSendService();

            Assert.False(fileSendService.CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();

            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var writeStream = File.Open("c:/" + guid + ".tmp",
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.Read);
            writeStream.Write(data, 0, 2);
            writeStream.Close();

            var readStream = File.Open("c:/" + guid + ".tmp",
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
            var fileSendService = new FileSendService();

            var statusCode = fileSendService
                .ProcessRequest("GET /" + guid + ".tmp HTTP/1.1",
                    new HttpResponse(zSocket), properties);
            File.Delete("c:/" + guid + ".tmp");
            Assert.Equal("200 OK", statusCode);
            zSocket.VerifySend(data, 2);
            zSocket.VerifySend(GetByte("HTTP/1.1 200 OK\r\n"),
                GetByteCount("HTTP/1.1 200 OK\r\n"));
            zSocket.VerifySend(GetByte("Content-Length: 2\r\n\r\n"),
                GetByteCount("Content-Length: 2\r\n\r\n"));
            zSocket.VerifySend(GetByte("Cache-Control: no-cache\r\n"),
                GetByteCount("Cache-Control: no-cache\r\n"));
            zSocket.VerifySend(GetByte("Content-Disposition: attachment"
                                       + "; filename = " + guid + ".tmp\r\n"),
                GetByteCount("Content-Disposition: attachment"
                             + "; filename = " + guid + ".tmp\r\n"));
            zSocket.VerifySend(GetByte("Content-Type: application/octet-stream\r\n"),
                GetByteCount("Content-Type: application/octet-stream\r\n"));
        }


        [Fact]
        public void Cant_Send_Data_Protected_Data()
        {
            var zSocket = new MockZSocket();
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
            var fileSendService = new FileSendService();
            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Server 403 Error Page</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<h1>403 Forbidden, Can not process request on port 5555</h1>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");
            var statusCode
                = fileSendService
                    .ProcessRequest("GET /pagefile.sys HTTP/1.1", 
                    new HttpResponse(zSocket),
                    properties);

            Assert.Equal("403 Forbidden", statusCode);
            zSocket.VerifySend(GetByte("HTTP/1.1 403 Forbidden\r\n"),
                GetByteCount("HTTP/1.1 403 Forbidden\r\n"));
            zSocket.VerifySend(GetByte("Cache-Control: no-cache\r\n"),
                GetByteCount("Cache-Control: no-cache\r\n"));
            zSocket.VerifySend(GetByte("Content-Type: text/html\r\n"),
                GetByteCount("Content-Type: text/html\r\n"));
            zSocket.VerifySend(GetByte("Content-Length: "
                                       + GetByteCount(correctOutput.ToString())
                                       + "\r\n\r\n"),
                GetByteCount("Content-Length: "
                             + GetByteCount(correctOutput.ToString())
                             + "\r\n\r\n"));

            zSocket.VerifySend(GetByte(correctOutput.ToString()),
                GetByteCount(correctOutput.ToString()));
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