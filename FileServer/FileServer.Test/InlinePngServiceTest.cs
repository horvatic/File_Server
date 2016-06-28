//using FileServer.Core;
//using Server.Core;
//using Xunit;

//namespace FileServer.Test
//{
//    public class InlinePngServiceTest
//    {
//        [Fact]
//        public void Make_Not_Null_Class()
//        {
//            Assert.NotNull(new InlinePngService());
//        }

//        [Theory]
//        [InlineData("GET /hello.png HTTP/1.1")]
//        [InlineData("GET /hi.png HTTP/1.0")]
//        public void Can_Process(string getRequest)
//        {
//            var mockFileSearch = new MockFileProcessor()
//                .StubExists(true);
//            var properties = new ServerProperties(@"c:/",
//                5555, new HttpResponse(), new ServerTime(),
//                new MockPrinter(),
//                new Readers
//                {
//                    DirectoryProcess = new MockDirectoryProcessor(),
//                    FileProcess = mockFileSearch
//                });
//            var inlinePngService = new InlinePngService();

//            Assert.True(inlinePngService
//                .CanProcessRequest(getRequest, properties));
//        }

//        [Theory]
//        [InlineData("GET /hello.gr HTTP/1.1")]
//        [InlineData("GET /hi.er HTTP/1.0")]
//        public void Cant_Process(string getRequest)
//        {
//            var mockFileSearch = new MockFileProcessor()
//                .StubExists(true);
//            var properties = new ServerProperties(@"c:/",
//                5555, new HttpResponse(), new ServerTime(),
//                new MockPrinter(),
//                new Readers
//                {
//                    DirectoryProcess = new MockDirectoryProcessor(),
//                    FileProcess = mockFileSearch
//                });
//            var inlinePngService = new InlinePngService();

//            Assert.False(inlinePngService.CanProcessRequest(getRequest, properties));
//        }

//        [Fact]
//        public void Send_Data()
//        {
//            var mockFileSearch = new MockFileProcessor()
//                .StubExists(true);
//            var properties = new ServerProperties(@"c:/",
//                5555, new HttpResponse(), new ServerTime(),
//                new MockPrinter(),
//                new Readers
//                {
//                    DirectoryProcess = new MockDirectoryProcessor(),
//                    FileProcess = mockFileSearch
//                });
//            var inlinePngService = new InlinePngService();

//            var httpResponces = inlinePngService
//                .ProcessRequest("GET /hello.png HTTP/1.1",
//                    new HttpResponse(),
//                    properties);

//            Assert.Equal(httpResponces.FilePath, "c:/" + "hello.png");
//            Assert.Equal(httpResponces.ContentDisposition, "inline");
//            Assert.Equal(httpResponces.Filename, "hello.png");
//            Assert.Equal(httpResponces.ContentType, "image/png");
//        }
//    }
//}