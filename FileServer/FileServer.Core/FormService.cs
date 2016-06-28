//using System;
//using System.Net;
//using System.Text;
//using Server.Core;

//namespace FileServer.Core
//{
//    public class FormService : IHttpServiceProcessor
//    {
//        public bool CanProcessRequest(string request, ServerProperties serverProperties)
//        {
//            var requestItem = CleanRequest(request);
//            return requestItem == "/form";
//        }

//        public IHttpResponse ProcessRequest(string request, IHttpResponse httpResponse,
//            ServerProperties serverProperties)
//        {
//            return request.Contains("GET /form") ? GetRequest(httpResponse) : PostRequest(request, httpResponse);
//        }

//        private IHttpResponse PostRequest(string request, IHttpResponse httpResponse)
//        {
//            var name = request.Remove(0, request.LastIndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
//            var firstName = WebUtility.UrlDecode(name.Substring(0, name.IndexOf("&", StringComparison.Ordinal))
//                .Replace("firstname=", ""));
//            var lastName =
//                WebUtility.UrlDecode(name.Substring(name.IndexOf("&", StringComparison.Ordinal) + 1)
//                    .Replace("lastname=", ""));

//            var formPage = new StringBuilder();
//            formPage.Append(@"First Name Submitted:<br>");
//            formPage.Append(WebUtility.HtmlEncode(firstName) + "<br>");
//            formPage.Append(@"Last Name Submitted:<br>");
//            formPage.Append(WebUtility.HtmlEncode(lastName) + "<br>");

//            httpResponse.HttpStatusCode = "200 OK";
//            httpResponse.CacheControl = "no-cache";
//            httpResponse.ContentType = "text/html";
//            httpResponse.Body = HtmlHeader() + formPage + HtmlTail();
//            httpResponse.ContentLength = Encoding
//                .ASCII.GetByteCount(httpResponse.Body);
//            return httpResponse;
//        }

//        private IHttpResponse GetRequest(IHttpResponse httpResponse)
//        {
//            var formPage = new StringBuilder();
//            formPage.Append(@"<form action=""form"" method=""post"">");
//            formPage.Append(@"First name:<br>");
//            formPage.Append(@"<input type=""text"" name=""firstname""><br>");
//            formPage.Append(@"Last name:<br>");
//            formPage.Append(@"<input type=""text"" name=""lastname""><br><br>");
//            formPage.Append(@"<input type=""submit"" value=""Submit"">");
//            formPage.Append(@"</form>");
//            httpResponse.HttpStatusCode = "200 OK";
//            httpResponse.CacheControl = "no-cache";
//            httpResponse.ContentType = "text/html";
//            httpResponse.Body = HtmlHeader() + formPage + HtmlTail();
//            httpResponse.ContentLength = Encoding
//                .ASCII.GetByteCount(httpResponse.Body);
//            return httpResponse;
//        }

//        private string CleanRequest(string request)
//        {
//            var parseVaulue = request.Contains("GET") ? "GET" : "POST";
//            var offsets = request.Contains("GET") ? 5 : 6;
//            if (request.Contains("HTTP/1.1"))
//                return "/" + request.Substring(request.IndexOf(parseVaulue + " /", StringComparison.Ordinal) + offsets,
//                    request.IndexOf(" HTTP/1.1", StringComparison.Ordinal) - offsets)
//                    .Replace("%20", " ");
//            return "/" + request.Substring(request.IndexOf(parseVaulue + " /", StringComparison.Ordinal) + offsets,
//                request.IndexOf(" HTTP/1.0", StringComparison.Ordinal) - offsets)
//                .Replace("%20", " ");
//        }

//        private string HtmlHeader()
//        {
//            var header = new StringBuilder();
//            header.Append(@"<!DOCTYPE html>");
//            header.Append(@"<html>");
//            header.Append(@"<head><title>Vatic Form Page</title></head>");
//            header.Append(@"<body>");
//            return header.ToString();
//        }

//        private string HtmlTail()
//        {
//            var tail = new StringBuilder();
//            tail.Append(@"</body>");
//            tail.Append(@"</html>");
//            return tail.ToString();
//        }
//    }
//}