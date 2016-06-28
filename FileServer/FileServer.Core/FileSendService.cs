using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class FileSendService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request, ServerProperties serverProperties)
        {
            var readers = (Readers) serverProperties
                .ServiceSpecificObjectsWrapper;
            var requestItem = CleanRequest(request);
            var configManager = ConfigurationManager.AppSettings;
            if (configManager.AllKeys.Any(key => requestItem.EndsWith(configManager[key])))
            {
                return false;
            }

            return serverProperties.CurrentDir != null &&
                   readers.FileProcess
                       .Exists(serverProperties.CurrentDir + requestItem.Substring(1))
                   && request.Contains("GET /");
        }

        public string ProcessRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var requestItem = CleanRequest(request).Substring(1);
            try
            {
                var readers = (Readers) serverProperties
                    .ServiceSpecificObjectsWrapper;
                httpResponse.SendHeaders(new List<string>
                {
                    "HTTP/1.1 200 OK\r\n",
                    "Cache-Control: no-cache\r\n",
                    "Content-Type: application/octet-stream\r\n",
                    "Content-Disposition: attachment"
                    + "; filename = "
                    + requestItem.Remove(0, requestItem.LastIndexOf('/') + 1)
                    + "\r\n",
                    "Content-Length: " + readers.FileProcess
                        .FileSize(serverProperties.CurrentDir
                                  + requestItem)
                    + "\r\n\r\n"
                });

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
            catch (Exception)
            {
                var errorPage = new StringBuilder();
                errorPage.Append(@"<!DOCTYPE html>");
                errorPage.Append(@"<html>");
                errorPage.Append(@"<head><title>Vatic Server 403 Error Page</title></head>");
                errorPage.Append(@"<body>");
                errorPage.Append(@"<h1>403 Forbidden, Can not process request on port " + serverProperties.Port +
                                 "</h1>");
                errorPage.Append(@"</body>");
                errorPage.Append(@"</html>");
                httpResponse.SendHeaders(new List<string>
                {
                    "HTTP/1.1 403 Forbidden\r\n",
                    "Cache-Control: no-cache\r\n",
                    "Content-Type: text/html\r\n",
                    "Content-Length: "
                    + (Encoding.ASCII.GetByteCount(errorPage.ToString())) +
                    "\r\n\r\n"
                });

                httpResponse.SendBody(Encoding
                    .ASCII.GetBytes(errorPage.ToString()),
                    Encoding.ASCII.GetByteCount(errorPage.ToString()));

                return "403 Forbidden";
            }
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