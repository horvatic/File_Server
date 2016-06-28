using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class DirectoryService : IHttpServiceProcessor
    {
        public bool CanProcessRequest(string request, ServerProperties serverProperties)
        {
            var fileProcessors =
                ((Readers)serverProperties.ServiceSpecificObjectsWrapper);
            var requestItem = CleanRequest(request);
            var configManager = ConfigurationManager.AppSettings;
            if (configManager.AllKeys.Any(key => requestItem.EndsWith(configManager[key]))
                || request.Contains("POST /"))
            {
                return false;
            }
            return serverProperties.CurrentDir != null &&
                   fileProcessors.DirectoryProcess.Exists(serverProperties.CurrentDir + requestItem.Substring(1));
        }

        public string ProcessRequest(string request, IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var fileProcessors =
                ((Readers)serverProperties.ServiceSpecificObjectsWrapper);
            var requestItem = CleanRequest(request);
            requestItem = requestItem.Substring(1);
            var listing = DirectoryContents(requestItem,
                fileProcessors.DirectoryProcess,
                serverProperties.CurrentDir,
                serverProperties.Port);
            httpResponse.SendHeaders(new List<string>
            {
                "HTTP/1.1 200 OK\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: text/html\r\n",
                "Content-Length: "
                + (GetByteCount(listing)) +
                "\r\n\r\n"
            });

            httpResponse.SendBody(GetByte(listing),
                GetByteCount(listing));
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
            header.Append(@"<head><title>Vatic Server Directory Listing</title></head>");
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

        private string DirectoryContents(string dir, IDirectoryProcessor reader, string root, int port)
        {
            var directoryContents = new StringBuilder();
            var files = reader.GetFiles(root + dir);
            foreach (var replacedBackSlash in files.Select(file => file.Replace('\\', '/')))
            {
                directoryContents.Append(@"<br><a href=""http://localhost:" + port + "/" +
                                         replacedBackSlash.Replace(" ", "%20")
                                             .Remove(replacedBackSlash.IndexOf(root, StringComparison.Ordinal),
                                                 replacedBackSlash.IndexOf(root, StringComparison.Ordinal) + root.Length) +
                                         @""" >" +
                                         replacedBackSlash.Remove(0, replacedBackSlash.LastIndexOf('/') + 1)
                                         + "</a>");
            }
            var subDirs = reader.GetDirectories(root + dir);
            foreach (var replacedBackSlash in subDirs.Select(subDir => subDir.Replace('\\', '/')))
            {
                directoryContents.Append(@"<br><a href=""http://localhost:" + port + "/" +
                                         replacedBackSlash.Replace(" ", "%20")
                                             .Remove(replacedBackSlash.IndexOf(root, StringComparison.Ordinal),
                                                 replacedBackSlash.IndexOf(root, StringComparison.Ordinal) + root.Length) +
                                         @""" >" +
                                         replacedBackSlash.Remove(0, replacedBackSlash.LastIndexOf('/') + 1)
                                         + "</a>");
            }
            return HtmlHeader() + directoryContents + HtmlTail();
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