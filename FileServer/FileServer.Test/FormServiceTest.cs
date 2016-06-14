using System.Text;
using FileServer.Core;
using Server.Core;
using Xunit;

namespace FileServer.Test
{
    public class FormServiceTest
    {
        [Fact]
        public void Make_Not_Null_Class()
        {
            Assert.NotNull(new FormService());
        }

        [Theory]
        [InlineData("GET /form HTTP/1.1")]
        [InlineData("GET /form HTTP/1.0")]
        [InlineData("POST /form HTTP/1.1")]
        [InlineData("POST /form HTTP/1.0")]
        public void Can_Process(string getRequest)
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var formService = new FormService();

            Assert.True(formService
                .CanProcessRequest(getRequest, properties));
        }

        [Fact]
        public void Send_Data_Get_Request()
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var formService = new FormService();

            var httpResponces = formService
                .ProcessRequest("GET /form HTTP/1.1",
                    new HttpResponse(), properties);

            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Form Page</title></head>");
            correctOutput.Append(@"<body>");
            correctOutput.Append(@"<form action=""form"" method=""post"">");
            correctOutput.Append(@"First name:<br>");
            correctOutput.Append(@"<input type=""text"" name=""firstname""><br>");
            correctOutput.Append(@"Last name:<br>");
            correctOutput.Append(@"<input type=""text"" name=""lastname""><br><br>");
            correctOutput.Append(@"<input type=""submit"" value=""Submit"">");
            correctOutput.Append(@"</form>");
            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            Assert.Equal(correctOutput.ToString(), httpResponces.Body);
        }

        [Fact]
        public void Send_Data_Post_Request()
        {
            var mockFileSearch = new MockFileProcessor();
            mockFileSearch.StubExists(true);
            var properties = new ServerProperties(@"c:/",
                5555, new HttpResponse(), new ServerTime(),
                new MockPrinter(),
                new Readers
                {
                    DirectoryProcess = new DirectoryProcessor(),
                    FileProcess = mockFileSearch
                });
            var formService = new FormService();

            var httpResponces = formService.ProcessRequest("POST /form HTTP/1.1\r\n" +
                                                           "Host: localhost:8080\r\n" +
                                                           "Connection: keep-alive\r\n" +
                                                           "Content-Length: 33\r\n" +
                                                           "Cache - Control: max - age = 0\r\n" +
                                                           "Accept: text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,*/*;q=0.8\r\n" +
                                                           "Origin: http://localhost:8080\r\n" +
                                                           "Upgrade-Insecure-Requests: 1\r\n" +
                                                           "User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36\r\n" +
                                                           "Content-Type: application/x-www-form-urlencoded\r\n" +
                                                           "Referer: http://localhost:8080/form\r\n" +
                                                           "Accept-Encoding: gzip, deflate\r\n" +
                                                           "Accept-Language: en-US,en;q=0.8\r\n\r\n" +
                                                           "firstname=John%26"
                                                           + "&lastname=Walsher%26", new HttpResponse(), properties);

            var correctOutput = new StringBuilder();
            correctOutput.Append(@"<!DOCTYPE html>");
            correctOutput.Append(@"<html>");
            correctOutput.Append(@"<head><title>Vatic Form Page</title></head>");
            correctOutput.Append(@"<body>");

            correctOutput.Append(@"First Name Submitted:<br>");
            correctOutput.Append(@"John&amp;<br>");
            correctOutput.Append(@"Last Name Submitted:<br>");
            correctOutput.Append(@"Walsher&amp;<br>");


            correctOutput.Append(@"</body>");
            correctOutput.Append(@"</html>");

            Assert.Equal(correctOutput.ToString(), httpResponces.Body);
        }
    }
}