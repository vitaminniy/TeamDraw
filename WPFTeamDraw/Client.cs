using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WPFTeamDraw
{
    public class Client
    {
        public static readonly byte trequest = 11;
        public static readonly byte lrequest = 12;
        public static readonly byte prequest = 13;
        public static readonly byte lprequest = 14;

        public ConcurrentQueue<byte[]> sendq = new ConcurrentQueue<byte[]>();

        IPAddress ipAddr;
        IPEndPoint ipEndPoint;
        Socket sender;

        Thread rthread, wthread;

        public Client(string ip, int port)
        {
            ipAddr = IPAddress.Parse(ip);
            ipEndPoint = new IPEndPoint(ipAddr, port);

            sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
        }

        private void recv(byte[] buf, int offset, Socket handler)
        {
            while (offset < buf.Length) offset += handler.Receive(buf, offset, buf.Length - offset, SocketFlags.None);
        }

        public void Start()
        {
            sender.Connect(ipEndPoint);

            byte[] reply = new byte[1];
            sender.Receive(reply, 1, SocketFlags.None);

            if (reply[0] == 0) //Too many clients
            {
                System.Environment.Exit(0);
            }
            else
            {
                sender.Send(new byte[] { 27 });
            }

            //Now sync time
            byte[] time = new byte[8];
            long delay = -1;
            for (int i = 0; i < 3; i++)
            {
                long old = Util.CurrentTimeMillis();
                sender.Send(new byte[] { trequest });
                recv(time, 0, sender);
                long now = Util.CurrentTimeMillis();
                if (delay == -1 || now - old < delay)
                {
                    delay = now - old;
                    long server = BitConverter.ToInt64(time, 0);
                    Util.ServerTimeDifference = server - delay / 2 - old;
                }
            }

            rthread = new Thread(new ThreadStart(read));
            wthread = new Thread(new ThreadStart(write));
            rthread.Start();
            wthread.Start();
        }

        private void read()
        {
            try
            {
                byte[] rid = new byte[1];
                while (true)
                {
                    sender.Receive(rid, 1, SocketFlags.None);
                    if (rid[0] == lrequest)
                    {
                        byte[] data = new byte[18];
                        recv(data, 0, sender);
                        RLine rline = new RLine(data);
                        MainWindow.handleRLine(rline);
                    }
                    else if (rid[0] == prequest)
                    {
                        byte[] data = new byte[24];
                        recv(data, 0, sender);
                        RPoint rpoint = new RPoint(data);
                        MainWindow.handleRPoint(rpoint);
                    }
                    else if (rid[0] == lprequest)
                    {
                        byte[] data = new byte[8];
                        recv(data, 0, sender);
                        MainWindow.handleLPoint(BitConverter.ToInt64(data, 0));
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    sender.Close();
                }
                catch (Exception) { }

                //End of client. Do something... Like a warning box
                System.Environment.Exit(1);
            }
        }

        private void write()
        {
            try
            {
                while (true)
                {
                    byte[] data;
                    while (sendq.TryDequeue(out data))
                    {
                        sender.Send(data);
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception)
            {
                try
                {
                    sender.Close();
                } catch(Exception) {}
            }
        }
    }
}
