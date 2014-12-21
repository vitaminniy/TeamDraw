using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C = System.Console;


namespace TeamDrawServer
{
    class Program
    {
        static int port = 2055;
        static string ip = null;

        private static readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        static void Main(string[] args)
        {
            C.WriteLine("Starting up TeamDraw server");

            try
            {
                if (args.Length >= 1) port = int.Parse(args[0]);
                if (args.Length >= 2) ip = args[1];
                if (args.Length >= 3) throw new Exception();
            }
            catch (Exception) {
                C.WriteLine("Could not parse arguments.\nHelp:\nTeamDrawServer.exe [port] [ip]");
                return;
            }

            Server server;
            try
            {
                server = new Server(ip, port);
            }
            catch (Exception ex)
            {
                C.WriteLine("Could not use IP/Port, ex: {0}", ex);
                return;
            }

            C.WriteLine("Server starting up on {0}:{1}", ip == null ? "0.0.0.0" : ip, port);

            try
            {
                server.Serve();
            }
            catch (Exception ex)
            {
                C.WriteLine("Error while binding to IP/Port, ex: {0}", ex);
                return;
            }

            while (true)
            {
                string input = C.ReadLine().ToLowerInvariant();
                if (input == "exit" || input == "stop") System.Environment.Exit(0); 
            }
        }
    }
}
