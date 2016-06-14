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
                5555, new HttpResponse(), new ServerTime(),
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
        public void Cant_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
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
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var fileSendService = new FileSendService();

            var httpResponces = fileSendService.ProcessRequest("GET /hello.exe HTTP/1.1", new HttpResponse(), properties);

            Assert.Equal(httpResponces.FilePath, "c:/" + "hello.exe");
            Assert.Equal(httpResponces.ContentDisposition, "attachment");
            Assert.Equal(httpResponces.Filename, "hello.exe");
            Assert.Equal(httpResponces.ContentType, "application/octet-stream");
        }


        [Fact]
        public void Cant_Send_Data_Protected_Data()
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
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
            var httpResponces = fileSendService.ProcessRequest("GET /pagefile.sys HTTP/1.1", new HttpResponse(),
                properties);

            Assert.Equal(correctOutput.ToString(), httpResponces.Body);
            Assert.Equal("403 Forbidden", httpResponces.HttpStatusCode);
        }
    }
}