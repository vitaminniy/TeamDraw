using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamDrawServer
{
    class Server
    {

        private static readonly byte pversion = 27;
        private static readonly byte trequest = 11;

        IPHostEntry ipHost;
        IPAddress ipAddr;
        IPEndPoint ipEndPoint;
        Socket sListener;
        Thread sThread;
        public Server(string ip, int port)
        {
            ipHost = Dns.GetHostEntry(ip);
            ipAddr = ipHost.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddr, port);
            sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Serve()
        {
            lock (this)
            {
                if (sThread != null)
                {
                    Console.WriteLine("Server thread already running, cant server again");
                    return;
                }

                sListener.Bind(ipEndPoint); //May couse exception if port is already in use
                sListener.Listen(10);

                sThread = new Thread(new ThreadStart(run));
                sThread.Start();
            }
        }

        private void run()
        {
            try
            {
                while (true)
                {
                    Socket handler = sListener.Accept();
                    int ta, ta1;
                    ThreadPool.GetAvailableThreads(out ta, out ta1);
                    if (ta < 1) //We dont have any available threads to handle another client
                    {
                        handler.Send(new byte[] { 0 });
                        handler.Close();
                    }
                    else
                    {
                        handler.Send(new byte[] { 1 });
                        ThreadPool.QueueUserWorkItem(handleSocket, handler);
                    }
                    
                }
            }
            catch (Exception) { }
        }

        private void handleSocket(Object sock)
        {
            Socket handler = (Socket) sock;

            byte[] pbytes = new byte[1];
            handler.Receive(pbytes);
            if (pbytes[0] != pversion) //Wrong protocol version or a wrong client
            {
                handler.Close();
                return;
            }

            //Start syncing time
            byte[] tbytes = new byte[8];
            for (int i = 0; i < 3; i++)
            {
                handler.Receive(pbytes);
                if (pbytes[0] != trequest)
                {
                    IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine("Client {0} behaved weirdly during time sync, disconnecting",
                        remoteIpEndPoint.Address);

                    handler.Close();
                    return;
                }
                tbytes = BitConverter.GetBytes(Program.CurrentTimeMillis());
                handler.Send(tbytes);
            }
            //Time synced... Most likely ;-)
            

        }

        public void Stop()
        {
            lock (this)
            {
                if (sThread == null)
                {
                    Console.WriteLine("Cant stop the server: server is already stopped");
                    return;
                }

                try
                {
                    sListener.Close();
                }
                catch (Exception) { }

                sThread.Abort();
                sThread = null;
            }
        }
    }
}
