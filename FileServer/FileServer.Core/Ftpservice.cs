using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Server.Core;

namespace FileServer.Core
{
    public class Ftpservice : IHttpServiceProcessor
    {
        private string _directory;
        private bool _directRequest;
        private string _file;
        private bool _headerRemoved;
        private string _packetBound;

        public bool CanProcessRequest(string request,
            ServerProperties serverProperties)
        {
            var requestItem = CleanRequest(request);
            if (requestItem != "/upload")
                return serverProperties.CurrentDir != null
                       && IsDirect(request, serverProperties);
            _directRequest = false;
            return true;
        }

        public string ProcessRequest(string request, IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            return request.Contains("GET /") && request.IndexOf("GET /", StringComparison.Ordinal) == 0
                ? GetRequest(request, httpResponse)
                : PostRequest(request, httpResponse, serverProperties);
        }

        private bool IsDirect(string request,
            ServerProperties serverProperties)
        {
            var requestItem = CleanRequest(request);
            var configManager = ConfigurationManager.AppSettings;
            if (configManager.AllKeys.Any(key => requestItem.EndsWith(configManager[key]))
                && !request.Contains("POST /"))
            {
                return false;
            }
            if (!request.Contains("POST /"))
                return false;
            var fullpath
                = (serverProperties.CurrentDir + requestItem.Substring(1));
            _file = fullpath.Substring(fullpath
                .LastIndexOf("/", StringComparison.Ordinal) + 1);
            var copyDistance =
                _directory = fullpath.Substring(0, fullpath
                    .LastIndexOf("/", StringComparison.Ordinal) + 1);
            _directRequest = true;
            _headerRemoved = false;
            return true;
        }

        private string GetPacketBound(string request)
        {
            var packetSplit = request.Substring(request.IndexOf("boundary=----"
                , StringComparison.Ordinal) + 13);
            packetSplit = packetSplit.Remove(packetSplit.IndexOf("\r\n"
                , StringComparison.Ordinal));
            return packetSplit;
        }

        private void SetDirectoryAndFile(string request, ServerProperties serverProperties)
        {
            if (request.Contains("name=\"saveLocation\"\r\n\r\n") && _directory == null)
            {
                _directory = serverProperties.CurrentDir;
                _directory += CleanPost(request, "name=\"saveLocation\"\r\n\r\n", "\r\n");
                if (!_directory.EndsWith("/"))
                    _directory += "/";
            }
            if (request.Contains("filename=\"") && _file == null)
            {
                _file = CleanPost(request, "filename=\"", "\"\r\n");
            }
        }

        private string RemoveHeaderAndSetPacketBound(string request)
        {
            _headerRemoved = true;
            _packetBound = GetPacketBound(request);
            return request.Substring(request.IndexOf("\r\n\r\n"
                , StringComparison.Ordinal) + 4);
        }


        private string ProcessRequestWithPath(string data,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var readers = (Readers) serverProperties
                .ServiceSpecificObjectsWrapper;
            SetDirectoryAndFile(data, serverProperties);
            if (_directory != null && _file != null
                && (!readers.DirectoryProcess.Exists(_directory)
                    || readers.FileProcess.Exists(_directory + _file))
                )
            {
                return SendHeaderAndBody("409 Conflict", httpResponse,
                    PostWebPage("Could not make item"));
            }
            if (_directory != null && _file != null
                && data.Contains("Content-Type: "))
                return ProcessData(data, httpResponse,
                    serverProperties);
            return "200 OK";
        }


        private string RemoveContentDisposition(string data)
        {
            var processedData = data;
            if (!_directRequest)
            {
                processedData = processedData
                    .Substring(processedData.IndexOf("Content-Disposition: form-data;"
                                                     + @" name=""fileToUpload"""
                        , StringComparison.Ordinal));
                processedData = processedData.Substring(processedData.IndexOf("\r\n"
                    , StringComparison.Ordinal) + 2);
            }
            else
            {
                processedData = processedData
                    .Substring(processedData.IndexOf("Content-Disposition: form-data;"
                        , StringComparison.Ordinal));
                processedData = processedData.Substring(processedData.IndexOf("\r\n"
                    , StringComparison.Ordinal) + 2);
            }
            return processedData;
        }

        private string RemoveContentType(string data)
        {
            var processedData = data;
            processedData = processedData.Substring(data.IndexOf("Content-Type: "
                , StringComparison.Ordinal));
            processedData = processedData.Substring(processedData.IndexOf("\r\n\r\n"
                , StringComparison.Ordinal) + 4);
            return processedData;
        }

        private string ProcessData(string data,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var processedData = data;
            processedData = RemoveContentDisposition(processedData);
            processedData = RemoveContentType(processedData);

            return SaveFile(processedData, httpResponse,
                serverProperties);
        }

        private string SaveFile(string data,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            if (_file == "" || _directory == "")
            {
                return SendHeaderAndBody("409 Conflict", httpResponse,
                    PostWebPage("Could not make item"));
            }
            var sendData = data;
            if (sendData.EndsWith("\r\n------" + _packetBound + "--\r\n"))
                sendData = sendData.Replace("\r\n------" + _packetBound + "--\r\n", "");
            serverProperties.Io.PrintToFile(sendData, _directory + _file);
            return SendHeaderAndBody("201 Created", httpResponse,
                PostWebPage("Item Made"));
        }

        private string PostRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            if (!_directRequest)
                return UsedUpLoad(request,
                    httpResponse, serverProperties);
            return DirectRequest(request,
                httpResponse, serverProperties);
        }

        private string DirectRequest(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var data = request.Contains("POST /") && !_headerRemoved
                ? RemoveHeaderAndSetPacketBound(request)
                : request;
            if (data == "")
            {
                return SendHeaderAndBody("201 Created", httpResponse,
                    PostWebPage("Item Made"));
            }
            if (data.Contains(@"Content-Disposition: form-data; name=""file""")
                || data.Contains(@"Content-Disposition: form-data; name=""fileToUpload"""))
                return ProcessRequestWithPath(data, httpResponse, serverProperties);
            return SaveFile(data, httpResponse,
                serverProperties);
        }

        private string UsedUpLoad(string request,
            IHttpResponse httpResponse,
            ServerProperties serverProperties)
        {
            var data = request.Contains("POST /upload HTTP/1.1\r\n")
                       && _directory == null && _file == null
                ? RemoveHeaderAndSetPacketBound(request)
                : request;
            if (data == "")
            {
                return SendHeaderAndBody("201 Created", httpResponse,
                    PostWebPage("Item Made"));
            }
            if ((data.Contains(@"Content-Disposition: form-data; name=""saveLocation""")
                 || data.Contains(@"Content-Disposition: form-data; name=""fileToUpload"""))
                && (_directory == null || _file == null))
                return ProcessRequestWithPath(data, httpResponse, serverProperties);
            return SaveFile(data, httpResponse,
                serverProperties);
        }

        private string CleanPost(string request, string head, string tail)
        {
            var cleanInput = request.Substring(request.IndexOf(head
                , StringComparison.Ordinal) + head.Length);
            cleanInput = cleanInput.Remove(cleanInput.IndexOf(tail, StringComparison.Ordinal));

            return cleanInput;
        }


        private string GetRequest(string request,
            IHttpResponse httpResponse)
        {
            var uploadPage = new StringBuilder();
            uploadPage.Append(HtmlHeader());
            uploadPage.Append(@"<form action=""upload"" method=""post"" enctype=""multipart/form-data"">");
            uploadPage.Append(@"Select Save Location<br>");
            uploadPage.Append(@"<input type=""text"" name=""saveLocation""><br>");
            uploadPage.Append(@"Select File To Upload<br>");
            uploadPage.Append(@"<input type=""file"" name=""fileToUpload"" id=""fileToUpload""><br>");
            uploadPage.Append(@"<input type=""submit"" value=""Submit"">");
            uploadPage.Append(@"</form>");
            uploadPage.Append(HtmlTail());

            return SendHeaderAndBody("200 OK", httpResponse,
                uploadPage.ToString());
        }

        private string UploadPage()
        {
            var uploadPage = new StringBuilder();
            uploadPage.Append(@"<form action=""upload"" method=""post"" enctype=""multipart/form-data"">");
            uploadPage.Append(@"Select Save Location<br>");
            uploadPage.Append(@"<input type=""text"" name=""saveLocation""><br>");
            uploadPage.Append(@"Select File To Upload<br>");
            uploadPage.Append(@"<input type=""file"" name=""fileToUpload"" id=""fileToUpload""><br>");
            uploadPage.Append(@"<input type=""submit"" value=""Submit"">");
            uploadPage.Append(@"</form>");

            return uploadPage.ToString();
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
            header.Append(@"<head><title>Vatic File Upload</title></head>");
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

        private string PostWebPage(string message)
        {
            return HtmlHeader() + message + "<br>" + UploadPage() + HtmlTail();
        }

        private string SendHeaderAndBody(string statusCode,
            IHttpResponse response, string webPage)
        {
            response.SendHeaders(new List<string>
            {
                "HTTP/1.1 " + statusCode + "\r\n",
                "Cache-Control: no-cache\r\n",
                "Content-Type: text/html\r\n",
                "Content-Length: "
                + (Encoding.ASCII.GetByteCount(webPage)) +
                "\r\n\r\n"
            });

            response.SendBody(Encoding
                .ASCII.GetBytes(webPage),
                Encoding.ASCII.GetByteCount(webPage));
            return statusCode;
        }
    }
}