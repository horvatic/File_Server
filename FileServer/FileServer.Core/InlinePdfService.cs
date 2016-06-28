using System;
using System.Collections.Generic;
using Server.Core;

namespace FileServer.Core
{
    public class InlinePdfService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request,
            ServerProperties serverProperties)
        {
            var readers = (Readers) serverProperties
                .ServiceSpecificObjectsWrapper;
            var requestItem = CleanRequest(request);
            return serverProperties.CurrentDir != null &&
                   readers.FileProcess
                       .Exists(serverProperties.CurrentDir + requestItem) &&
                   requestItem.EndsWith(".pdf")
                   && request.Contains("GET /");
        }

        public string ProcessRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var readers = (Readers) serverProperties
                .ServiceSpecificObjectsWrapper;
            var requestItem = CleanRequest(request);
            var headers = new List<string>
            {"HTTP/1.1 200 OK\r\n", "Cache-Control: no-cache\r\n"};
            if (readers.FileProcess
                .FileSize(serverProperties.CurrentDir + requestItem)
                > 1000000)
            {
                headers.Add("Content-Type: application/octet-stream\r\n");
                headers.Add("Content-Disposition: attachment"
                            + "; filename = "
                            + requestItem.Remove(0, requestItem.LastIndexOf('/') + 1)
                            + "\r\n");
            }
            else
            {
                headers.Add("Content-Type: application/pdf\r\n");
                headers.Add("Content-Disposition: inline"
                            + "; filename = "
                            + requestItem.Remove(0, requestItem.LastIndexOf('/') + 1)
                            + "\r\n");
            }
            headers.Add("Content-Length: " + readers.FileProcess
                .FileSize(serverProperties.CurrentDir
                          + requestItem)
                        + "\r\n\r\n");
            httpResponse.SendHeaders(headers);
            using (var fileStream = readers.FileProcess
                .GetFileStream(serverProperties.CurrentDir + requestItem))
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
                return request.Substring(request.IndexOf("GET /", StringComparison.Ordinal) + 5,
                    request.IndexOf(" HTTP/1.1", StringComparison.Ordinal) - 5)
                    .Replace("%20", " ");
            return request.Substring(request.IndexOf("GET /", StringComparison.Ordinal) + 5,
                request.IndexOf(" HTTP/1.0", StringComparison.Ordinal) - 5)
                .Replace("%20", " ");
        }
    }
}