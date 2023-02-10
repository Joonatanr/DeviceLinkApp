using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLinkApp
{
    class Program
    {
        private const int DLPort = 1711;
        private static string myIp;

        static void Main(string[] args)
        {
            // Get the Name of HOST  
            string hostName = Dns.GetHostName();
            Console.WriteLine(hostName);

            // Get the IP from GetHostByName method of dns class.
            myIp = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine("IP Address is : " + myIp);


            BackgroundWorker senderWorker = new BackgroundWorker();
            senderWorker.DoWork += new DoWorkEventHandler(SenderThread);
            senderWorker.RunWorkerAsync();

            Console.ReadLine();

            /* TODO : Clean up connections etc.... */
        }

        private static void SenderThread(object sender, DoWorkEventArgs args )
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 1711);

            udpClient.Client.ReceiveTimeout = 1000;

            while (true)
            {
                System.Threading.Thread.Sleep(50);

                Byte[] sendBytes1 = Encoding.ASCII.GetBytes("R/115");
                Byte[] sendBytes2 = Encoding.ASCII.GetBytes("R/103");
                Byte[] sendBytes3 = Encoding.ASCII.GetBytes("R/40");
                try
                {
                    //udpClient.Send(sendBytes1, sendBytes1.Length, "192.168.1.164", DLPort);
                    //udpClient.Send(sendBytes2, sendBytes2.Length, "192.168.1.164", DLPort);
                    //udpClient.Send(sendBytes3, sendBytes3.Length, "192.168.1.164", DLPort);

                    //udpClient.Send(sendBytes1, sendBytes1.Length, myIp, DLPort);
                    //udpClient.Send(sendBytes2, sendBytes2.Length, myIp, DLPort);
                    udpClient.Send(sendBytes3, sendBytes3.Length, myIp, DLPort);


                }
                catch (Exception e)
                {
                    Console.WriteLine("Tx failed");
                    //Console.WriteLine(e.ToString());
                }

                try
                {
                    byte[] received = udpClient.Receive(ref remoteEndPoint);
                    Console.WriteLine(Encoding.UTF8.GetString(received));
                }
                catch(Exception e)
                {
                    Console.WriteLine("Rx failed");
                    //Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
