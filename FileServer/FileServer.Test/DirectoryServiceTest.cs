using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class DirectoryServiceTest
    {
        [Fact]
        public void Make_New_Class_Not_Null()
        {
            Assert.NotNull(new DirectoryService());
        }

        [Theory]
        [InlineData("GET / HTTP/1.1")]
        [InlineData("GET / HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockDirSearch = new MockDirectoryProcessor();
            mockDirSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirSearch,
                    FileProcess = new FileProcessor()
                });
            var directoryServer = new DirectoryService();

            Assert.True(directoryServer.CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /form HTTP/1.1")]
        public void Cant_Process_form(string getRequest)
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
            var directoryServer = new DirectoryService();

            Assert.False(directoryServer.CanProcessRequest(getRequest, properties));
        }

        [Theory]
        [InlineData("GET /fe.pdf HTTP/1.1")]
        [InlineData("GET /we HTTP/1.0")]
        public void Cant_Process(string getRequest)
        {
            var mockDirSearch = new MockDirectoryProcessor();
            mockDirSearch.StubExists(false);
            var properties = new ServerProperties(null,
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockDirSearch,
                    FileProcess = new MockFileProcessor()
                });
            var directoryServer = new DirectoryService();

            Assert.False(directoryServer.CanProcessRequest(getRequest, properties));
        }


        [Theory]
        [InlineData("GET / HTTP/1.0")]
        [InlineData("GET / HTTP/1.1")]
        public void Get_Directory_Listing(string getRequest)
        {
            var mockRead = new MockDirectoryProcessor()
                .StubGetDirectories(new[] {"Home/dir 1", "Home/dir2"})
                .StubGetFiles(new[] {"Home/file 1", "Home/file2", "Home/file3"});
            var zSocket = new MockZSocket()
                .StubSentToReturn(10)
                .StubReceive(getRequest)
                .StubConnect(true);
            zSocket = zSocket.StubAcceptObject(zSocket);
            var properties = new ServerProperties(@"Home", 8080,
                new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = mockRead,
                    FileProcess = new MockFileProcessor()
                });
            var directoryServer = new DirectoryService();
            var httpResponse = directoryServer.ProcessRequest(getRequest, properties.DefaultResponse.Clone(), properties);

            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Server Directory Listing</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<br><a href=""http://localhost:8080/file%201"" >file 1</a>");
            correctOutput.Append(@"<br><a href=""http://localhost:8080/file2"" >file2</a>");
            correctOutput.Append(@"<br><a href=""http://localhost:8080/file3"" >file3</a>");
            correctOutput.Append(@"<br><a href=""http://localhost:8080/dir%201"" >dir 1</a>");
            correctOutput.Append(@"<br><a href=""http://localhost:8080/dir2"" >dir2</a>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            Assert.Equal(correctOutput.ToString(), httpResponse.Body);
        }
    }
}