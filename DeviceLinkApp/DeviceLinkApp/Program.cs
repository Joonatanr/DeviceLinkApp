﻿using System;
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
        static void Main(string[] args)
        {
            BackgroundWorker senderWorker = new BackgroundWorker();
            senderWorker.DoWork += new DoWorkEventHandler(SenderThread);
            senderWorker.RunWorkerAsync();
            
            
            //Creates a UdpClient for reading incoming data.
            UdpClient receivingUdpClient = new UdpClient(8081);

            //Creates an IPEndPoint to record the IP Address and port number of the sender.
            // The IPEndPoint will allow you to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("This is the message you received " +
                                              returnData.ToString());
                    Console.WriteLine("This message was sent from " +
                                                RemoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                RemoteIpEndPoint.Port.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            Console.ReadLine();
        }

        private static void SenderThread(object sender, DoWorkEventArgs args )
        {
            UdpClient udpClient = new UdpClient();

            while (true)
            {
                System.Threading.Thread.Sleep(500);
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there");
                try
                {
                    udpClient.Send(sendBytes, sendBytes.Length, "127.0.0.1", 8081);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
