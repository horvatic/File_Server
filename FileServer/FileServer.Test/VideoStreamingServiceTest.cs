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
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);

            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), 
                new ServerTime(),
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
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(),
                new ServerTime(),
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
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            var httpResponce = videoStream.ProcessRequest("GET /hello.mp4 HTTP/1.1", new HttpResponse(), properties);
            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Video</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<video width=""320"" height=""240"" controls>");
            correctOutput.Append(@"<source src=""http://127.0.0.1:5555/hello.mp4.vaticToMp4");
            correctOutput.Append(@""" type=""video/mp4"">");
            correctOutput.Append(@"</video>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            Assert.Equal(correctOutput.ToString(), httpResponce.Body);
            Assert.Equal("200 OK", httpResponce.HttpStatusCode);
            Assert.Equal("no-cache", httpResponce.CacheControl);
            Assert.Equal("text/html", httpResponce.ContentType);
        }

        [Fact]
        public void Send_Data_Large_Pdf()
        {
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers()
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var videoStream = new VideoStreamingService();

            var httpResponces = videoStream
                .ProcessRequest("GET /hello.mp4.vaticToMp4 HTTP/1.1", 
                new HttpResponse(),
                properties);

            Assert.Equal("c:/" + "hello.mp4", httpResponces.FilePath);
            Assert.Equal("inline", httpResponces.ContentDisposition);
            Assert.Equal("hello.mp4", httpResponces.Filename);
            Assert.Equal("video/mp4", httpResponces.ContentType);
        }
    }
}