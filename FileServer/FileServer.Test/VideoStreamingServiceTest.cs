﻿using System;
using System.IO;
using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class VideoStreamingServiceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new VideoStreamingService());
        }

        [Theory]
        [InlineData("GET /hello.mp4 HTTP/1.1")]
        [InlineData("GET /hi.mp4 HTTP/1.0")]
        [InlineData("GET /hello.vaticToMp4 HTTP/1.1")]
        [InlineData("GET /hi.vaticToMp4 HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            Assert.True(videoStream.CanProcessRequest(getRequest,
                properties));
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
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            Assert.False(videoStream.CanProcessRequest(getRequest,
                properties));
        }

        [Fact]
        public void Get_Video_Page()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();
            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var writeStream = File.Open("c:/" + guid + ".mp4",
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.Read);
            writeStream.Write(data, 0, 2);
            writeStream.Close();

           var mockFileSearch = new MockFileProcessor()
                .StubExists(true)
                .StubFileSize(2);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            var statusCode = videoStream
                .ProcessRequest("GET /"+ guid + ".mp4 HTTP/1.1", 
                new HttpResponse(zSocket), properties);
            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Video</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<video width=""320"" height=""240"" controls>");
            correctOutput.Append(@"<source src=""http://127.0.0.1:5555/"+ guid + ".mp4.vaticToMp4");
            correctOutput.Append(@""" type=""video/mp4"">");
            correctOutput.Append(@"</video>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            File.Delete("c:/" + guid + ".mp4");
            Assert.Equal("200 OK", statusCode);
            zSocket.VerifySend(GetByte("HTTP/1.1 200 OK\r\n"),
                GetByteCount("HTTP/1.1 200 OK\r\n"));
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

        [Fact]
        public void Send_Data_Of_Video()
        {
            var zSocket = new MockZSocket();
            var guid = Guid.NewGuid();
            var data = new byte[1024];
            data[0] = 0;
            data[1] = 1;
            var writeStream = File.Open("c:/" + guid + ".mp4",
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.Read);
            writeStream.Write(data, 0, 2);
            writeStream.Close();

            var readStream = File.Open("c:/" + guid + ".mp4",
                FileMode.Open, FileAccess.Read,
                FileShare.Read);
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true)
                .StubGetFileStream(readStream)
                .StubFileSize(2);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            var statusCode = videoStream
                .ProcessRequest("GET /"+ guid + ".vaticToMp4 HTTP/1.1",
                new HttpResponse(zSocket),
                properties);
            File.Delete("c:/" + guid + ".mp4");

            Assert.Equal("200 OK", statusCode);
            zSocket.VerifySend(data, 2);
            zSocket.VerifySend(GetByte("HTTP/1.1 200 OK\r\n"),
                GetByteCount("HTTP/1.1 200 OK\r\n"));
            zSocket.VerifySend(GetByte("Content-Length: 2\r\n\r\n"),
                GetByteCount("Content-Length: 2\r\n\r\n"));
            zSocket.VerifySend(GetByte("Cache-Control: no-cache\r\n"),
                GetByteCount("Cache-Control: no-cache\r\n"));
            zSocket.VerifySend(GetByte("Content-Disposition: inline"
                                       + "; filename = " + guid + ".vaticToMp4\r\n"),
                GetByteCount("Content-Disposition: inline"
                             + "; filename = " + guid + ".vaticToMp4\r\n"));
            zSocket.VerifySend(GetByte("Content-Type: video/mp4\r\n"),
                GetByteCount("Content-Type: video/mp4\r\n"));
        }

        [Fact]
        public void Send_Data_Of_Error_Video()
        {
            var guid = Guid.NewGuid();
            var zSocket = new MockZSocket();
            
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true)
                .StubGetFileStream(null)
                .StubFileSize(2);

            var properties = new ServerProperties(@"c:/",
                5555, new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            var statusCode = videoStream
                .ProcessRequest("GET /" + guid + ".vaticToMp4 HTTP/1.1",
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