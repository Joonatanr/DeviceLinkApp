using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceLinkGui
{
    public class DeviceLinkHandler
    {
        public delegate void printHandler(string str);
        public printHandler TextPrinter;
        
        private int myPort;
        private static string myIp;
        private bool isStop = false;

        public DeviceLinkHandler(int port)
        {
            //Constructor
            myPort = port;
        }

        public void Run()
        {
            // Get the Name of HOST  
            string hostName = Dns.GetHostName();
            debugPrintLine(hostName);

            // Get the IP from GetHostByName method of dns class.
            myIp = Dns.GetHostByName(hostName).AddressList[0].ToString();
            debugPrintLine("IP Address is : " + myIp);

            //Set up worker thread.
            // Create a thread   
            Thread commThread = new Thread(new ThreadStart(this.SenderThread));
            // Start thread  
            commThread.Start();
        }

        public void Close()
        {
            isStop = true;
        }

        private void SenderThread()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 1711);

            udpClient.Client.ReceiveTimeout = 1000;

            while (isStop == false)
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
                    udpClient.Send(sendBytes3, sendBytes3.Length, myIp, myPort);


                }
                catch (Exception e)
                {
                    debugPrintLine("Tx failed");
                    //Console.WriteLine("Tx failed");
                    //Console.WriteLine(e.ToString());
                }

                try
                {
                    byte[] received = udpClient.Receive(ref remoteEndPoint);
                    debugPrintLine(Encoding.UTF8.GetString(received));
                    //Console.WriteLine(Encoding.UTF8.GetString(received));
                }
                catch (Exception e)
                {
                    debugPrintLine("Rx failed");
                    //Console.WriteLine("Rx failed");
                    //Console.WriteLine(e.ToString());
                }
            }

            udpClient.Close();
        }

        private void debugPrint(string str)
        {
            if(TextPrinter != null)
            {
                TextPrinter.Invoke(str);
            }
        }

        private void debugPrintLine(string str)
        {
            if (TextPrinter != null)
            {
                TextPrinter.Invoke(str + Environment.NewLine);
            }
        }
    }
}
