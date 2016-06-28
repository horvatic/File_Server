//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Text;
//using Server.Core;

namespace FileServer.Core
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return 0;
        }
    }
}

//namespace FileServer.Core
//{
//    public class Program
//    {
//        private const int LowerBoundPort = 2000;
//        private const int UpperBoundPort = 65000;
//        private const string PortOption = "-p";
//        private const string DirectoryOption = "-d";
//        private const string LogOption = "-l";

//        public static int Main(string[] args)
//        {
//            RunServer(MakeServer(args, new DefaultPrinter()));
//            return 0;
//        }

//        public static void RunServer(IMainServer runningServer)
//        {
//            var closeServerProcess = new ClosingServerHandler(runningServer);
//            Console.CancelKeyPress += closeServerProcess.ShutdownProcess;

//            if (runningServer == null) return;
//            do
//            {
//                runningServer.Run();
//            } while (runningServer.AcceptingNewConn);
//        }

//        private static string ParseArg(string option, IReadOnlyList<string> args)
//        {
//            for (var i = 0; i < args.Count; i++)
//            {
//                if (args[i] == option && i + 1 < args.Count)
//                    return args[i + 1];
//            }
//            return null;
//        }

//        private static bool IsArrayClean(IReadOnlyList<string> args)
//        {
//            var outref = 0;
//            if (args.Count > 0)
//                return
//                    args.Where(arg => arg.Contains("-") && !int.TryParse(arg, out outref))
//                        .All(arg => arg == PortOption || arg == DirectoryOption || arg == LogOption);
//            return false;
//        }

//        public static IMainServer MakeServer(string[] args, IPrinter io)
//        {
//            var port = ParseArg(PortOption, args);
//            var directory = ParseArg(DirectoryOption, args);
//            var log = ParseArg(LogOption, args);
//            var arrayClean = IsArrayClean(args);
//            try
//            {
//                if (log != null && arrayClean)
//                    io.Log = log;
//                if (port != null && directory != null && arrayClean)
//                    return MakedirectoryServer(port, directory, io);
//                if (port != null && arrayClean)
//                    return HelloWorldServer(port, io);
//                io.Print(WrongNumberOfArgs());
//                return null;
//            }
//            catch (Exception)
//            {
//                io.Print("Another Server is running on that port");
//                return null;
//            }
//        }

//        public static IMainServer MakedirectoryServer(string chosenPort, string homeDirectory, IPrinter io)
//        {
//            var cleanHomeDir = homeDirectory.Replace('\\', '/');
//            var port = PortWithinRange(chosenPort, io);
//            if (port == -1) return null;
//            if (!VaildDrive(cleanHomeDir, io)) return null;
//            var endPoint = new IPEndPoint((IPAddress.Loopback), port);
//            var zSocket = new DefaultZSocket(endPoint);
//            var properties = new ServerProperties(cleanHomeDir, 
//                port, new HttpResponse(), new ServerTime(), io,
//                new Readers
//                {
//                    FileProcess = new FileProcessor(),
//                    DirectoryProcess = new DirectoryProcessor()
//                });
//            return new MainServer(zSocket, properties, 
//                new HttpServiceFactory(new Service404()),
//                new DefaultRequestProcessor(), 
//                new DefaultSender(), 
//                new List<string>() { "FileServer.Core" },
//                 new List<Assembly>() { Assembly.GetExecutingAssembly() });
//        }

//        public static IMainServer HelloWorldServer(string port, IPrinter io)
//        {
//            var portConverted = PortWithinRange(port, io);
//            if (portConverted == -1) return null;
//            var endPoint = new IPEndPoint((IPAddress.Loopback), portConverted);
//            var zSocket = new DefaultZSocket(endPoint);
//            var properties = new ServerProperties(null,
//                portConverted,
//                new HttpResponse(), new ServerTime(), io,
//                new Readers
//                {
//                    FileProcess =  new FileProcessor(),
//                    DirectoryProcess = new DirectoryProcessor()
//                });
//            return new MainServer(zSocket, properties, 
//                new HttpServiceFactory(new Service404()), 
//                new DefaultRequestProcessor(), 
//                new DefaultSender(),
//                new List<string>() { "FileServer.Core" },
//                 new List<Assembly>() { Assembly.GetExecutingAssembly() });
//        }

//        private static bool VaildDrive(string dir, IPrinter io)
//        {
//            if (Directory.Exists(dir))
//            {
//                return true;
//            }
//            io.Print("Not a vaild directory");
//            return false;
//        }

//        private static int PortWithinRange(string port, IPrinter io)
//        {
//            int portconvert;
//            if (int.TryParse(port, out portconvert))
//            {
//                if (portconvert >= LowerBoundPort && portconvert <= UpperBoundPort) return portconvert;
//                io.Print(GetInvaildPortError());
//                return -1;
//            }
//            io.Print(GetInvaildPortError());
//            return -1;
//        }

//        private static string WrongNumberOfArgs()
//        {
//            var error = new StringBuilder();
//            error.Append("Invaild Number of Arguments.\n");
//            error.Append("Can only be -p PORT\n");
//            error.Append("or -p PORT -d DIRECTORY\n");
//            error.Append("Examples:\n");
//            error.Append("Server.exe -p 8080 -d C:/\n");
//            error.Append("Server.exe -d C:/HelloWorld -p 5555\n");
//            error.Append("Server.exe -p 9999");

//            return error.ToString();
//        }

//        private static string GetInvaildPortError()
//        {
//            var error = new StringBuilder();
//            error.Append("Invaild Port Detected.");
//            error.Append("Vaild Ports 2000 - 65000");

//            return error.ToString();
//        }
//    }
//}