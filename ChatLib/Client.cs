using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatLib
{
    public class Client
    {
        #region fields
        public readonly IPEndPoint listeningIPEndPoint;
        public readonly List<IPEndPoint> connectedClientsIP;
        public readonly Socket listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public bool IsLeave { get; private set; }
        public IPEndPoint LastConnect { get; private set; }
        #endregion

        public Client()
        {
            connectedClientsIP = new List<IPEndPoint>();
            listeningIPEndPoint = GetLocalIPEndPoint(ChatWindow.GetPortValue());
            bool isPortCorrect = false;
            while (!isPortCorrect)
            {
                try
                {
                    listeningSocket.Bind(listeningIPEndPoint);
                    isPortCorrect = true;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("This port is used by someone else");
                    listeningIPEndPoint = GetLocalIPEndPoint(ChatWindow.GetPortValue());
                }
            }
        }

        public void StartWaiting()
        {
            Task waitingTask = new Task(Wait);
            waitingTask.Start();
        }

        public void Connect(IPEndPoint ipToConnect)
        {
            LastConnect = ipToConnect;
            if (!connectedClientsIP.Contains(LastConnect))
            {
                connectedClientsIP.Add(LastConnect);
                SendIPs(LastConnect);
            }
        }

        public void Disconnect()
        {
            SendMessage('-' + listeningIPEndPoint.ToString());
            connectedClientsIP.Clear();
        }

        public void ShowClients()
        {
            Console.WriteLine("Clients list");
            foreach (IPEndPoint ip in connectedClientsIP)
            {
                Console.WriteLine(ip.ToString());
            }
        }

        public void Exit()
        {
            Disconnect();
            IsLeave = true;
        }

        public void SendMessage(string message)
        {

            Byte[] data = Encoding.Unicode.GetBytes(message);

            foreach (IPEndPoint tmpClientIP in connectedClientsIP)
            {
                listeningSocket.SendTo(data, tmpClientIP);
            }
        }

        public void SendIPs(IPEndPoint ip)
        {
            StringBuilder ipList = new StringBuilder("*");
            ipList.Append(listeningIPEndPoint.Address.ToString());
            ipList.Append(':');
            ipList.Append(listeningIPEndPoint.Port.ToString());
            ipList.Append(" ");
            foreach (IPEndPoint tmpIP in connectedClientsIP)
            {
                ipList.Append(tmpIP.Address.ToString());
                ipList.Append(':');
                ipList.Append(tmpIP.Port.ToString());
                ipList.Append(" ");
            }

            byte[] data = Encoding.Unicode.GetBytes(ipList.ToString());

            listeningSocket.SendTo(data, ip);
        }

        private void Wait()
        {
            try
            {
                while (true)
                {
                    StringBuilder inputMessage = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];
                    EndPoint tmpIP = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref tmpIP);
                        inputMessage.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (listeningSocket.Available > 0);

                    IPEndPoint senderIP = tmpIP as IPEndPoint;

                    ConnectionManager.ProccessMessage(inputMessage, this);

                    if (inputMessage[0] != '*' && inputMessage[0] != '+' && inputMessage[0] != '-')
                    {
                        Console.WriteLine("[{0}:{1}] - {2}", senderIP.Address.ToString(),
                            senderIP.Port, inputMessage.ToString());
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10054)
                {
                    connectedClientsIP.Remove(LastConnect);
                    Console.WriteLine("This address is not used by anyone");
                    StartWaiting();
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private IPEndPoint GetLocalIPEndPoint(int port)
        {
            IPEndPoint result = null;
            string host = Dns.GetHostName();
            var ipList = Dns.GetHostEntry(host).AddressList;
            foreach (var ip in ipList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    result = new IPEndPoint(ip, port);
                }
            }
            return result;
        }

        public void LeaveChat()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
            }
        }
    }
}
