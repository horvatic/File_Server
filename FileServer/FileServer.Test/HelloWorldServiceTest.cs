//using System.Text;
//using FileServer.Core;
//using Server.Core;
//using Xunit;

//namespace FileServer.Test
//{
//    public class HelloWorldServiceTest
//    {
//        [Fact]
//        public void Make_Hello_World_Service_Not_Null()
//        {
//            var helloWorldService = new HelloWorldService();
//            Assert.NotNull(helloWorldService);
//        }

//        [Theory]
//        [InlineData("GET / HTTP/1.1")]
//        [InlineData("GET / HTTP/1.0")]
//        public void Can_Process_Request_HTTP(string request)
//        {
//            var serverProperties =
//                new ServerProperties(null,
//                    5555, new HttpResponse(), new ServerTime(),
//                    new MockPrinter());
//            var helloWorldService = new HelloWorldService();
//            Assert.True(helloWorldService
//                .CanProcessRequest(request, serverProperties));
//        }


//        [Theory]
//        [InlineData("GET / HTTP/1.1", "Hello")]
//        [InlineData("GET /k HTTP/1.1", null)]
//        [InlineData("GET /Hello HTTP/1.0", null)]
//        [InlineData("GET /sf.txt HTTP/1.0", null)]
//        public void Cant_Process_Request_HTTP_1_0(string request, string currentDir)
//        {
//            var serverProperties =
//                new ServerProperties(currentDir, 5555,
//                    new HttpResponse(),
//                    new ServerTime(), new MockPrinter());
//            var helloWorldService = new HelloWorldService();
//            Assert.False(helloWorldService
//                .CanProcessRequest(request, serverProperties));
//        }

//        [Fact]
//        public void OutPuts_Hello_World()
//        {
//            var correctOutput = new StringBuilder();
//            correctOutput.Append(@"<!DOCTYPE html>");
//            correctOutput.Append(@"<html>");
//            correctOutput.Append(@"<head><title>Vatic Server Hello World</title></head>");
//            correctOutput.Append(@"<body>");
//            correctOutput.Append(@"<h1>Hello World</h1>");
//            correctOutput.Append(@"</body>");
//            correctOutput.Append(@"</html>");
//            var httpPackage = new HttpResponse();
//            var serverProperties =
//                new ServerProperties(null, 5555,
//                    new HttpResponse(), new ServerTime(),
//                    new MockPrinter());
//            var helloWorldService = new HelloWorldService();
//            httpPackage =
//                (HttpResponse) helloWorldService
//                    .ProcessRequest("", httpPackage, serverProperties);
//            Assert.Equal(correctOutput.ToString(), httpPackage.Body);
//        }
//    }
//}