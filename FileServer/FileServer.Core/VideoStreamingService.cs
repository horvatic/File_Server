using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class VideoStreamingService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request, ServerProperties serverProperties)
        {
            var reader = (Readers)serverProperties
                .ServiceSpecificObjectsWrapper;
            var requestItem = CleanRequest(request);
            return serverProperties.CurrentDir != null &&
                   (reader.FileProcess.Exists(serverProperties.CurrentDir
                                              + requestItem) &&
                    (requestItem.EndsWith(".mp4")))
                   || (requestItem.EndsWith(".vaticToMp4")
                       &&
                       reader.FileProcess.Exists(serverProperties.CurrentDir
                                                 +
                                                 requestItem.Replace(
                                                     ".vaticToMp4", ""))
                       && request.Contains("GET /"));
        }

        public string ProcessRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var reader = (Readers)serverProperties
                .ServiceSpecificObjectsWrapper;
            var requestItem = CleanRequest(request);
            return !requestItem.EndsWith(".vaticToMp4") 
                ? SendHtml(requestItem, serverProperties, httpResponse) 
                : SendVideo(requestItem, serverProperties, httpResponse,
                reader);
        }

        private string SendHtml(string requestItem,
             ServerProperties serverProperties,
             IHttpResponse httpResponse)
        {
            var html = HtmlHeader() +
                                @"<video width=""320"" height=""240"" controls>" +
                                @"<source src=""http://127.0.0.1:" + serverProperties.Port + "/" +
                                requestItem.Substring(1) + ".vaticToMp4" +
                                @""" type=""video/mp4"">" +
                                "</video>"
                                + HtmlTail();
            httpResponse.SendHeaders(new List<string>
            {
                "HTTP/1.1 200 OK\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: text/html\r\n",
                "Content-Length: "
                + (Encoding.ASCII.GetByteCount(html)) +
                "\r\n\r\n"
            });

            httpResponse.SendBody(Encoding
                .ASCII.GetBytes(html),
                Encoding.ASCII.GetByteCount(html));
            return "200 OK";
        }

        private string SendVideo(string requestItem,
             ServerProperties serverProperties,
             IHttpResponse httpResponse,
             Readers reader)
        {
            var filePath = serverProperties.CurrentDir
                           + requestItem.Substring(1).Replace(".vaticToMp4", "");
            httpResponse.SendHeaders(new List<string>
            {
                "HTTP/1.1 200 OK\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: video/mp4\r\n",
                "Content-Disposition: inline"
                            + "; filename = "
                            + requestItem.Remove(0, 
                                requestItem.LastIndexOf('/') + 1)
                            + "\r\n",
                "Content-Length: "
                + reader
                    .FileProcess.FileSize(filePath) +
                "\r\n\r\n"
            });
            using (var fileStream = reader.FileProcess
                .GetFileStream(filePath))
            {
                var buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0,
                    1024)) > 0)
                {
                    httpResponse.SendBody(buffer, bytesRead);
                }
            }
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

        private string HtmlHeader()
        {
            var header = new StringBuilder();
            header.Append(@"<!DOCTYPE html>");
            header.Append(@"<html>");
            header.Append(@"<head><title>Vatic Video</title></head>");
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
    }
}