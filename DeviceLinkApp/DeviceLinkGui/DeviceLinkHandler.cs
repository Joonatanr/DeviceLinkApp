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

        public float CurrentAirspeed = 0.0f;
        public float CurrentAltitude = 0.0f;

        public DeviceLinkHandler(int port)
        {
            //Constructor
            myPort = port;
        }

        public void Run()
        {
            //Debug our parser
            string test1 = "A/40\\534.3/30\\100.4";
            byte[] testByte1 = System.Text.Encoding.UTF8.GetBytes(test1);

            Dictionary<int, string> parsedData = parseReceivePacket(testByte1);

            foreach(KeyValuePair<int, string> pair in parsedData)
            {
                debugPrint("Key : " + pair.Key.ToString());
                debugPrintLine(" Value : " + pair.Value);
            }



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
                Byte[] sendBytes3 = Encoding.ASCII.GetBytes("R/40/30");
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
                    //debugPrintLine("Tx failed");
                    //Console.WriteLine("Tx failed");
                    //Console.WriteLine(e.ToString());
                }

                try
                {
                    byte[] received = udpClient.Receive(ref remoteEndPoint);
                    handleReceivedPacket(received);
                    //debugPrintLine(Encoding.UTF8.GetString(received));
                    //Console.WriteLine(Encoding.UTF8.GetString(received));
                }
                catch (Exception e)
                {
                    //debugPrintLine("Rx failed");
                    //Console.WriteLine("Rx failed");
                    //Console.WriteLine(e.ToString());
                }
            }

            udpClient.Close();
        }


        private void handleReceivedPacket(byte[] packet)
        {
            Dictionary<int, string> myDictionary = parseReceivePacket(packet);
            string val;

            /* Soo lets try a simple approach at first.*/
            if (myDictionary.Keys.Contains(30))
            {
                val = myDictionary[30];
                float parsedValue;
                if (float.TryParse(val, out parsedValue))
                {
                    CurrentAirspeed = parsedValue;
                }
            }

            if (myDictionary.Keys.Contains(40))
            {
                val = myDictionary[40];
                float parsedValue;
                if (float.TryParse(val, out parsedValue))
                {
                    CurrentAltitude = parsedValue;
                }
            }
        }

        private Dictionary<int, string> parseReceivePacket(byte[] packet)
        {
            int ix;
            int key;
            string value;

            string temp;
            Dictionary<int, string> res = new Dictionary<int, string>();


            if (packet[0] == 'A')
            {
                ix = 1;
                while (ix < packet.Length)
                {
                    /* First we are looking for a key.. */

                    if (packet[ix] == '/')
                    {
                        ix++;
                        temp = "";

                        /* Now this should be followed by a key. */
                        while ((ix < packet.Length) && char.IsDigit((char)packet[ix]))
                        {
                            temp += (char)packet[ix];
                            ix++;
                        }

                        /* Lets try and parse out the key. */
                        key = int.Parse(temp);

                        /* Now lets see if there is a value following it. */
                        if (packet[ix] == '\\')
                        {
                            temp = "";
                            ix++;

                            while (ix < packet.Length && !(packet[ix] == '/' || packet[ix] == '\\'))
                            {
                                temp += (char)packet[ix];
                                ix++;
                            }
                            
                            value = temp;
                            res.Add(key, value);
                        }
                    }
                    else
                    {
                        ix++;
                    }
                }
            }

            return res;
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
