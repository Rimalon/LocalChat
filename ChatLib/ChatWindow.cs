using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatLib
{
    public class ChatWindow
    {
        private readonly Client client;

        public ChatWindow()
        {
            client = new Client();
            StartChating();

        }
        
        public void StartChating()
        {
            ShowHelp();
            client.StartWaiting();
            while (!client.IsLeave)
            {
                try
                {
                    string message = Console.ReadLine();

                    if (message == "")
                    {
                        throw new IOException();
                    }

                    if (message[0] == '*' || message[0] == '+' || message[0] == '-')
                    {
                        throw new ArgumentException();
                    }

                    switch (message)
                    {
                        
                        case "Connect":
                        {
                            client.Connect(GetFormattedIPEndPoint());
                            break;
                        }

                        case "Disconnect":
                        {
                            client.Disconnect();
                            break;
                        }
                        case "Clients":
                        {
                            client.ShowClients();
                            break;
                        }
                        case "Help":
                        {
                            ShowHelp();
                            break;
                        }
                        case "Exit":
                        {
                            client.Exit();
                            break;
                        }
                        default:
                        {
                            client.SendMessage(message);
                            break;
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Incorrect input");
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Incorrect input, don't use + * - at the begining of the message");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            client.LeaveChat();
        }

        private void ShowHelp()
        {
            Console.WriteLine("\n" +
                              "Connect  -  to connect to other clients\n" +
                              "Disconnect  -  to disconnect from this lobby\n" +
                              "Clients  -  to show who is in this lobby now\n" +
                              "Help  -  to show this help window\n" +
                              "Exit  -  to exit from chat\n");
        }

        public static int GetPortValue()
        {
            int result = 0;
            bool isInitCorrect = false;
            while (!isInitCorrect)
            {
                try
                {
                    Console.Write("Input your port: ");
                    int tmpPortVal = Int32.Parse(Console.ReadLine());
                    if (tmpPortVal >= 1 && tmpPortVal <= 65535)
                    {
                        result = tmpPortVal;
                        isInitCorrect = true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Incorrect input port value, try again!");
                }
            }

            return result;
        }
        
        private IPEndPoint GetFormattedIPEndPoint()
        {
            bool isCorrectInput = false;
            while (!isCorrectInput)
            {
                try
                {
                    Console.Write("Input IP:port ");
                    string inStr = Console.ReadLine();
                    IsCorrectFormatIPAndPort(inStr);
                    return ConvertToIP(inStr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Input IP & port in this format: X.X.X.X:Y");
                }

            }

            return new IPEndPoint(IPAddress.None, 0);
        }

        private void IsCorrectFormatIPAndPort(string inIPAndPort)
        {
            try
            {
                int i = 0;
                for (int j = 0; j < 4; ++j)
                {
                    string temp = "";
                    while (inIPAndPort[i] != '.' && inIPAndPort[i] != ':')
                    {
                        temp += inIPAndPort[i];
                        i++;
                    }

                    try
                    {
                        if (Convert.ToInt32(temp) < 0 || Convert.ToInt32(temp) > 255)
                        {
                            throw new FormatException();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FormatException();
                    }

                    i++;
                }

                String tmpPort = "";
                while (i < inIPAndPort.Length)
                {
                    tmpPort += inIPAndPort[i];
                    i++;
                }

                try
                {
                    Convert.ToInt32(tmpPort);
                }
                catch (Exception e)
                {
                    throw new FormatException();
                }
            }
            catch (Exception e)
            {
                throw new FormatException();
            }
        }
        
        public static IPEndPoint ConvertToIP(string inStr)
        {
            try
            {
                int colonInd = inStr.LastIndexOf(':');
                return new IPEndPoint(
                    IPAddress.Parse(inStr.Substring(0, colonInd)),
                    Convert.ToInt32(inStr.Substring(colonInd + 1)));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error format of IP");
                throw new ArgumentException();
            }
        }
    }
}
