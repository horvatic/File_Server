using System;
using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class FtpserviceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new Ftpservice());
        }

        [Theory]
        [InlineData("GET /upload HTTP/1.1")]
        [InlineData("GET /upload HTTP/1.0")]
        [InlineData("POST /upload HTTP/1.1")]
        [InlineData("POST /upload HTTP/1.0")]
        [InlineData("POST /ThisNeedsToBeUpLoaded.png HTTP/1.0")]
        [InlineData("POST /ThisNeedsToBeUpLoaded.png HTTP/1.1")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            mockFileSearch.StubExists(false);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            Assert.True(ftpservice
                .CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /ThisNeedsToBeUpLoaded.png HTTP/1.0")]
        [InlineData("GET /ThisNeedsToBeUpLoaded.png HTTP/1.1")]
        [InlineData("GET /upload/ HTTP/1.1")]
        public void Cant_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            Assert.False(ftpservice
                .CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data_Get_Request()
        {
            var mockFileSearch = new MockFileProcessor();
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice.ProcessRequest("GET /upload HTTP/1.1", new HttpResponse(), properties);

            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic File Upload</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<form action=""upload"" method=""post"" enctype=""multipart/form-data"">");
            correctOutput.Append(@"Select Save Location<br>");
            correctOutput.Append(@"<input type=""text"" name=""saveLocation""><br>");
            correctOutput.Append(@"Select File To Upload<br>");
            correctOutput.Append(@"<input type=""file"" name=""fileToUpload"" id=""fileToUpload""><br>");
            correctOutput.Append(@"<input type=""submit"" value=""Submit"">");
            correctOutput.Append(@"</form>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            Assert.Equal(correctOutput.ToString(), httpResponces.Body);
        }

        [Fact]
        public void Send_Data_Post_Request_Check_If_File_Exist_It_Dosnt()
        {
            var request = new StringBuilder();
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("u6t\r\n");
            request.Append("------WebKitFormBoundaryqmueWCP8RQqHnEKH\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"blogBanner.png\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "IHDR  s  ?   ?s   sRGB ???   gAMA  ???a   	pHYs  t  t?fx  ??IDATx^??{?%e???N?9???'N???????.E.");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--");
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice.ProcessRequest(request.ToString(), new HttpResponse(), properties);

            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
        }

        [Fact]
        public void Send_Data_Post_Request_Check_If_File_Exist_It_Does()
        {
            var request = new StringBuilder();
            request.Append("------WebKitFormBoundaryqmueWCP8RQqHnEKH");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("u6t\r\n");
            request.Append("------WebKitFormBoundaryqmueWCP8RQqHnEKH\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"blogBanner.png\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "IHDR  s  ?   ?s   sRGB ???   gAMA  ???a   	pHYs  t  t?fx  ??IDATx^??{?%e???N?9???'N???????.E.");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice.ProcessRequest(request.ToString(), new HttpResponse(), properties);

            Assert.Equal("409 Conflict", httpResponces.HttpStatusCode);
        }


        [Fact]
        public void Send_Data_Post_Request_Save_File()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/"
                                                  + gid + ".txt");
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_With_Header()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/"
                                                  + gid + ".txt");
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_With_Header_Split_Request()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            var body = new StringBuilder();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n");
            body.Append(
                "?PNG\r\nHello");
            body.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(body.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/"
                                                  + gid + ".txt");
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_With_Header_Split_Request_Data_In_Header()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            var body = new StringBuilder();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n");
            request.Append(
                "?PNG\r\n");
            body.Append("Hello\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(body.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\n", "c:/ZZZ/" + gid + ".txt");
            io.VerifyPrintToFile("Hello", "c:/ZZZ/" + gid + ".txt");
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_With_Header_Not_Request_Data_In_Header()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            var body = new StringBuilder();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            body.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            body.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            body.Append("ZZZ\r\n");
            body.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            body.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            body.Append("Content-Type: image/png\r\n\r\n");
            body.Append(
                "?PNG\r\n");
            body.Append("Hello\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(body.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/"
                                                  + gid + ".txt");
        }

        [Fact]
        public void Message_Has_Content_Type()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            var body = new StringBuilder();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ\r\n");
            body.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            body.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            body.Append("Content-Type: image/png\r\n\r\n");
            body.Append(
                "?PNG\r\n");
            body.Append("Content-Type: image/png\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(body.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nContent-Type: image/png",
                "c:/ZZZ/" + gid + ".txt");
        }

        [Fact]
        public void Send_Data_Post_Body_Sent_Later()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            var body = new StringBuilder();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            body.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            body.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            body.Append("Content-Type: image/png\r\n\r\n");
            body.Append(
                "?PNG\r\n");
            body.Append("Hello\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(body.ToString(), new HttpResponse(),
                    properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/"
                                                  + gid + ".txt");
        }

        [Fact]
        public void Directory_Does_Not_Exsit()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n");
            request.Append(
                "?PNG\r\n");
            request.Append("Hello\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(false);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("409 Conflict", httpResponces.HttpStatusCode);
        }


        [Fact]
        public void No_File_Sent()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"\"\r\n");
            request.Append("Content-Type: application/octet-stream\r\n\r\n");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("409 Conflict", httpResponces.HttpStatusCode);
        }

        [Fact]
        public void No_Content_Type_In_Message()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /upload HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Connection: keep-alive\r\n" +
                           "Content-Length: 79841\r\n" +
                           "Cache-Control: max-age = 0\r\n" +
                           "Accept: text/html,application/xhtml+xml,application/xml; q=0.9,image/webp,*/*;q=0.8\r\n" +
                           "Origin: http://localhost:8080\r\n" +
                           "Upgrade-Insecure-Requests: 1\r\n" +
                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n" +
                           "Referer: http://localhost:8080/upload" +
                           "Accept-Encoding: gzip, deflate" +
                           "Accept-Language: en-US,en;q=0.8\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg");
            request.Append("Content-Disposition: form-data; name=\"saveLocation\"\r\n\r\n");
            request.Append("ZZZ/\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"fileToUpload\"; filename=\"\"\r\n");
            request.Append("Content-Type: application/octet-stream\r\n\r\n");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();

            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(), new HttpResponse(), properties);
            Assert.Equal("409 Conflict", httpResponces.HttpStatusCode);
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_Bound()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /ZZZ/testFile.txt HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Content-Length: 386\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg" +
                           "\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();
            ftpservice
                .CanProcessRequest(request.ToString(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/testFile.txt");
        }

        [Fact]
        public void Send_Data_Post_Request_Cant_Save_File_Bound()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /ZZZ/testFile.txt HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Content-Length: 386\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg" +
                           "\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(null,
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();
            Assert.False(ftpservice
                .CanProcessRequest(request.ToString(), properties));
           
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_Bound_Blank_Data()
        {
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /ZZZ/testFile.txt HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Content-Length: 386\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg" +
                           "\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            request.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();
            ftpservice.CanProcessRequest(request.ToString(),
                properties);
            ftpservice.ProcessRequest(request.ToString(),
                new HttpResponse(), properties);
            var httpResponces = ftpservice.ProcessRequest("",
                new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
        }

        [Fact]
        public void Send_Data_Post_Request_Save_File_Bound_Split()
        {
            var data = new StringBuilder();
            var request = new StringBuilder();
            var gid = Guid.NewGuid();
            request.Append("POST /ZZZ/testFile.txt HTTP/1.1\r\n" +
                           "Host: localhost: 8080\r\n" +
                           "Content-Length: 386\r\n" +
                           "Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryVfPQpsTmmlrqQLLg" +
                           "\r\n\r\n");
            request.Append("------WebKitFormBoundaryVfPQpsTmmlrqQLLg\r\n");
            request.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + gid + ".txt\"\r\n");
            request.Append("Content-Type: image/png\r\n\r\n?PNG\r\n");
            request.Append(
                "Hello");
            data.Append(
                "Hello");
            data.Append("\r\n------WebKitFormBoundaryVfPQpsTmmlrqQLLg--\r\n");
            var mockFileSearch = new MockFileProcessor()
                .StubExists(false);
            var mockDirectoySearch = new MockDirectoryProcessor()
                .StubExists(true);
            var io = new MockPrinter();
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(), io,
                new Readers
                {
                    DirectoryProcess = mockDirectoySearch,
                    FileProcess = mockFileSearch
                });
            var ftpservice = new Ftpservice();
            ftpservice
                .CanProcessRequest(request.ToString(), properties);
            ftpservice
                .ProcessRequest(request.ToString(),
                    new HttpResponse(), properties);
            var httpResponces = ftpservice
                .ProcessRequest(data.ToString(),
                    new HttpResponse(), properties);
            Assert.Equal("201 Created", httpResponces.HttpStatusCode);
            io.VerifyPrintToFile("?PNG\r\nHello", "c:/ZZZ/testFile.txt");
            io.VerifyPrintToFile("Hello", "c:/ZZZ/testFile.txt");
        }
    }
}