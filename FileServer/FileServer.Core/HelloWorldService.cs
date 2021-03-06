﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class HelloWorldService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request,
            ServerProperties serverProperties)
        {
            var requestItem = CleanRequest(request);
            var configManager = ConfigurationManager.AppSettings;
            if (configManager.AllKeys.Any(key => requestItem.EndsWith(configManager[key])))
            {
                return false;
            }
            return ((request.Contains("GET / HTTP/1.1")
                     || request.Contains("GET / HTTP/1.0"))
                    && serverProperties.CurrentDir == null);
        }

        public string ProcessRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var helloWorldHtml = new StringBuilder();
            helloWorldHtml.Append(@"<!DOCTYPE html>");
            helloWorldHtml.Append(@"<html>");
            helloWorldHtml.Append(@"<head><title>Vatic Server Hello World</title></head>");
            helloWorldHtml.Append(@"<body>");
            helloWorldHtml.Append(@"<h1>Hello World</h1>");
            helloWorldHtml.Append(@"</body>");
            helloWorldHtml.Append(@"</html>");
            httpResponse.SendHeaders(new List<string>
            {
                "HTTP/1.1 200 OK\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: text/html\r\n",
                "Content-Length: "
                + (Encoding.ASCII
                    .GetByteCount(helloWorldHtml.ToString())) +
                "\r\n\r\n"
            });

            httpResponse.SendBody(Encoding
                .ASCII.GetBytes(helloWorldHtml.ToString()),
                Encoding.ASCII.GetByteCount(helloWorldHtml.ToString()));
            return "200 OK";
        }

        private string CleanRequest(string request)
        {
            if (request.Contains("HTTP/1.1"))
                return "/" + request.Substring(request.IndexOf("GET /", StringComparison.Ordinal) + 5,
                    request.IndexOf(" HTTP/1.1", StringComparison.Ordinal) - 5)
                    .Replace("%20", " ");
            return "/" + request.Substring(request.IndexOf("GET /", StringComparison.Ordinal) + 5,
                request.IndexOf(" HTTP/1.0", StringComparison.Ordinal) - 5)
                .Replace("%20", " ");
        }
    }
}