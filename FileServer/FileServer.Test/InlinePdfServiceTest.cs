using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class InlinePdfServiceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new InlinePdfService());
        }

        [Theory]
        [InlineData("GET /hello.pdf HTTP/1.1")]
        [InlineData("GET /hi.pdf HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor()
                .StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePdfService = new InlinePdfService();

            Assert.True(inlinePdfService
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
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePdfService = new InlinePdfService();

            Assert.False(inlinePdfService
                .CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data_Small_Pdf()
        {
            var mockFileSearch = new MockFileProcessor()
                .StubFileSize(1).StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePdfService = new InlinePdfService();

            var httpResponces = inlinePdfService
                .ProcessRequest("GET /hello.pdf HTTP/1.1",
                    new HttpResponse(),
                    properties);

            Assert.Equal(httpResponces.FilePath, "c:/" + "hello.pdf");
            Assert.Equal(httpResponces.ContentDisposition, "inline");
            Assert.Equal(httpResponces.Filename, "hello.pdf");
            Assert.Equal(httpResponces.ContentType, "application/pdf");
        }

        [Fact]
        public void Send_Data_Large_Pdf()
        {
            var mockFileSearch = new MockFileProcessor()
                .StubFileSize(10000001);
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new MockDirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var inlinePdfService = new InlinePdfService();

            var httpResponces = inlinePdfService
                .ProcessRequest("GET /hello.pdf HTTP/1.1",
                    new HttpResponse(),
                    properties);

            Assert.Equal("c:/" + "hello.pdf", httpResponces.FilePath);
            Assert.Equal("attachment", httpResponces.ContentDisposition);
            Assert.Equal("hello.pdf", httpResponces.Filename);
            Assert.Equal("application/octet-stream",
                httpResponces.ContentType);
        }
    }
}