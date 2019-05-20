using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatLib
{
    static class ConnectionManager
    {

        public static void ProccessMessage(StringBuilder inputMessage, Client client)
        {
            if (inputMessage[0] == '*')
            {
                foreach (IPEndPoint ip in GetListOfIPs(inputMessage))
                {
                    if (!client.connectedClientsIP.Contains(ip))
                    {
                        client.connectedClientsIP.Add(ip);
                    }
                }

                client.connectedClientsIP.Remove(client.listeningIPEndPoint);

                Console.Write("Now these people are in the lobby: ");
                foreach (IPEndPoint clientIP in client.connectedClientsIP)
                {
                    Console.Write(clientIP.ToString() + " ");
                    List<IPEndPoint> tmpList = new List<IPEndPoint>();
                    tmpList.AddRange(client.connectedClientsIP);
                    tmpList.Remove(clientIP);
                    tmpList.Add(client.listeningIPEndPoint);
                    StringBuilder ipStrList = new StringBuilder("+");

                    foreach (IPEndPoint ip in tmpList)
                    {
                        ipStrList.Append(ip.Address.ToString());
                        ipStrList.Append(':');
                        ipStrList.Append(ip.Port.ToString());
                        ipStrList.Append(" ");
                    }

                    byte[] ipdata = Encoding.Unicode.GetBytes(ipStrList.ToString());
                    client.listeningSocket.SendTo(ipdata, clientIP);
                }

                Console.WriteLine();
            }


            if (inputMessage[0] == '+')
            {
                Console.Write("Now these people are in the lobby: ");
                foreach (IPEndPoint ip in GetListOfIPs(inputMessage))
                {
                    if (!client.connectedClientsIP.Contains(ip))
                    {
                        client.connectedClientsIP.Add(ip);
                    }

                    Console.Write(ip.ToString() + " ");
                }


                Console.WriteLine();
            }

            if (inputMessage[0] == '-')
            {
                string tmpStrIP = inputMessage.ToString(1, inputMessage.Length - 1);
                client.connectedClientsIP.Remove(ChatWindow.ConvertToIP(tmpStrIP));
                Console.WriteLine("{0} disconnected", tmpStrIP);
            }
        }
        
        private static List<IPEndPoint> GetListOfIPs(StringBuilder builder)
        {
            List<IPEndPoint> result = new List<IPEndPoint>();
            int i = 1;
            string tmpIP = "";
            while (builder.Length > i)
            {
                if (builder[i] == ' ')
                {
                    int ipAddressLength = tmpIP.LastIndexOf(':');
                    result.Add(new IPEndPoint(
                        IPAddress.Parse(tmpIP.Substring(0, ipAddressLength)),
                        Convert.ToInt32(tmpIP.Substring(ipAddressLength + 1))));
                    tmpIP = "";
                    i++;
                }
                else
                {
                    tmpIP += builder[i];
                    i++;
                }
            }

            return result;
        }
        
    }
}
