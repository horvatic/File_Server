using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class FormService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request,
            ServerProperties serverProperties)
        {
            var requestItem = CleanRequest(request);
            return requestItem == "/form";
        }

        public string ProcessRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            return request.Contains("GET /form") ? GetRequest(httpResponse) : PostRequest(request, httpResponse);
        }

        private string GetRequest(IHttpResponse httpResponse)
        {
            var formPage = new StringBuilder();
            formPage.Append(HtmlHeader());
            formPage.Append(@"<form action=""form"" method=""post"">");
            formPage.Append(@"First name:<br>");
            formPage.Append(@"<input type=""text"" name=""firstname""><br>");
            formPage.Append(@"Last name:<br>");
            formPage.Append(@"<input type=""text"" name=""lastname""><br><br>");
            formPage.Append(@"<input type=""submit"" value=""Submit"">");
            formPage.Append(@"</form>");
            formPage.Append(HtmlTail());
            return SendHeaderAndBody(formPage.ToString(), httpResponse);
        }

        private string PostRequest(string request, IHttpResponse httpResponse)
        {
            var name = request
                .Remove(0, request.LastIndexOf("\r\n\r\n",
                    StringComparison.Ordinal) + 4);
            var firstName = WebUtility
                .UrlDecode(name.Substring(0, name.IndexOf("&",
                    StringComparison.Ordinal))
                    .Replace("firstname=", ""));
            var lastName =
                WebUtility.UrlDecode(name.Substring(name
                    .IndexOf("&", StringComparison.Ordinal) + 1)
                    .Replace("lastname=", ""));

            var formPage = new StringBuilder();
            formPage.Append(HtmlHeader());
            formPage.Append(@"First Name Submitted:<br>");
            formPage.Append(WebUtility.HtmlEncode(firstName) + "<br>");
            formPage.Append(@"Last Name Submitted:<br>");
            formPage.Append(WebUtility.HtmlEncode(lastName) + "<br>");
            formPage.Append(HtmlTail());
            return SendHeaderAndBody(formPage.ToString(), httpResponse);
        }

        private string SendHeaderAndBody(string formPage, 
            IHttpResponse httpResponse)
        {
            httpResponse.SendHeaders(new List<string>
            {
                "HTTP/1.1 200 OK\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: text/html\r\n",
                "Content-Length: "
                + (GetByteCount(formPage.ToString())) +
                "\r\n\r\n"
            });
            httpResponse.SendBody(GetByte(formPage.ToString()),
                GetByteCount(formPage.ToString()));
            return "200 OK";
        }

        private string CleanRequest(string request)
        {
            var parseVaulue = request.Contains("GET") ? "GET" : "POST";
            var offsets = request.Contains("GET") ? 5 : 6;
            if (request.Contains("HTTP/1.1"))
                return "/" + request.Substring(request.IndexOf(parseVaulue + " /", StringComparison.Ordinal) + offsets,
                    request.IndexOf(" HTTP/1.1", StringComparison.Ordinal) - offsets)
                    .Replace("%20", " ");
            return "/" + request.Substring(request.IndexOf(parseVaulue + " /", StringComparison.Ordinal) + offsets,
                request.IndexOf(" HTTP/1.0", StringComparison.Ordinal) - offsets)
                .Replace("%20", " ");
        }

        private string HtmlHeader()
        {
            var header = new StringBuilder();
            header.Append(@"<!DOCTYPE html>");
            header.Append(@"<html>");
            header.Append(@"<head><title>Vatic Form Page</title></head>");
            header.Append(@"<body>");
            return header.ToString();
        }

        private string HtmlTail()
        {
            var tail = new StringBuilder();
            tail.Append(@"</body>");
            tail.Append(@"</html>");
            return tail.ToString();
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