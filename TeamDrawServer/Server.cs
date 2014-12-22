using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private readonly object qlock = new object(); //Lock object for queue initializations

        private static readonly int maxclients = 5;

        private static readonly byte pversion = 27;

        private static readonly byte trequest = 11;
        private static readonly byte lrequest = 12;
        private static readonly byte prequest = 13;
        private static readonly byte lprequest = 14;

        private ConcurrentQueue<byte[]> mqueue = new ConcurrentQueue<byte[]>();

        private ConcurrentDictionary<Socket, ConcurrentQueue<byte[]>> aqueues 
            = new ConcurrentDictionary<Socket, ConcurrentQueue<byte[]>>();

        private int clients = 0;

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
                    Console.WriteLine("Server thread already running, cant serve again");
                    return;
                }

                sListener.Bind(ipEndPoint); //May cause exception if port is already in use
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
                    
                    bool allowNew = false;
                    if (clients < maxclients)
                    {
                        Interlocked.Increment(ref clients);
                        allowNew = true;
                    }
                    
                    try
                    {
                        if (!allowNew) //We dont have any available threads to handle another client
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
                    catch (Exception) { //Happens if connection was closed and we tried to send 1 byte.
                        if(allowNew) Interlocked.Decrement(ref clients);
                    }
                    
                }
            }
            catch (Exception) { } //Happens only when Accept fails. Shouldnt ever happen
        }

        private void handleSocket(Object sock)
        {
            Socket handler = (Socket) sock;
            IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;
            try
            {
                byte[] pbytes = new byte[1];
                handler.Receive(pbytes);
                if (pbytes[0] != pversion) //Wrong protocol version or a wrong client
                {
                    Console.WriteLine("Client {0} sent a wrong protocol version, disconnecting",
                        remoteIpEndPoint.Address);
                    throw new Exception();
                }

                //Start syncing time
                byte[] tbytes = new byte[8];
                for (int i = 0; i < 3; i++)
                {
                    handler.Receive(pbytes);
                    if (pbytes[0] != trequest)
                    {
                        
                        Console.WriteLine("Client {0} behaved weirdly during time sync, disconnecting",
                            remoteIpEndPoint.Address);

                        throw new Exception();
                    }
                    tbytes = BitConverter.GetBytes(Program.CurrentTimeMillis());
                    handler.Send(tbytes);
                }
                //Time synced... Most likely ;-)

                //Now we need to initialize a queue for our client, add it to socket->queue map and copy all data from main queue
                //Dont forget synchronization
                ConcurrentQueue<byte[]> queue;
                lock (qlock)
                {
                    queue = new ConcurrentQueue<byte[]>(mqueue);
                    aqueues.TryAdd(handler, queue);
                }

                ThreadPool.QueueUserWorkItem(writeSocket, handler);

                //Now read
                byte[] data = new byte[33];
                while (true)
                {
                    handler.Receive(data, 33, SocketFlags.None);
                    addData(data, queue);
                }
                
            }
            catch (Exception) { //Basically once socket is disconnected this happens.
                try
                {
                    handler.Close();
                }
                catch (Exception) { }
                Interlocked.Decrement(ref clients);
                ConcurrentQueue<byte[]> o;
                aqueues.TryRemove(handler, out o);
                Console.WriteLine("Disconnecting client...");
            }

        }

        private void writeSocket(Object sock)
        {
            Socket handler = (Socket)sock;
            ConcurrentQueue<byte[]> queue;
            aqueues.TryGetValue(handler, out queue);

            try
            {
                while (true)
                {
                    byte[] data;
                    while (queue.TryDequeue(out data))
                    {
                        handler.Send(data);
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception)
            {
                try
                {
                    handler.Close();
                }
                catch (Exception) { }
            }
        }

        private void addData(byte[] data, ConcurrentQueue<byte[]> ignore)
        {
            ICollection<ConcurrentQueue<byte[]>> queues;
            lock (qlock)
            {
                mqueue.Enqueue(data);
                queues = aqueues.Values;
            }

            foreach (ConcurrentQueue<byte[]> queue in queues)
            {
                if (queue != ignore) queue.Enqueue(data);
            }
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
