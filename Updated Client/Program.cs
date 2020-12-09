using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static readonly Socket ClientSocket = new Socket(
                             AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int Port = 100;

        private static IPAddress ServerIP;

        private static bool ServerIPSet = false;

        private static string username = "you";

        private const int BufferSize = 2048;

        private static bool _closing = false;

        private static int topWrite = 10;

        private static int leftRead = 0;

        static void Main()
        {
            Console.Title = "Client";
            while (!ServerIPSet)
            {
                Console.WriteLine("Input IP-adress for server");
                
                string userInputIP;
                userInputIP = Console.ReadLine();

                if (IPAddress.TryParse(userInputIP, out ServerIP))
                {
                    ServerIPSet = true;
                }
                else
                {
                    Console.WriteLine("Input IP is not a valid IP-adress");
                }
            }
            //Fastnar i ConnectToServer() tills lyckad connection
            ConnectToServer();
            RequestLoop();
           

        }

        private static void ConnectToServer()
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt:" + attempts);
                    ClientSocket.Connect(ServerIP, Port);
                    

                }
                catch (SocketException)
                {

                    Console.Clear();
                }
                Console.Clear();

            }
            Console.SetCursorPosition(0, topWrite);

            Console.WriteLine("Connected to server with IP-adress: {0}", ServerIP);
            Console.WriteLine();
            Console.WriteLine(@"Type ""/exit"" to properly disconnect client");
            Console.WriteLine();
            Console.WriteLine(@"Do you want to set an username?");
            Console.WriteLine(@"If yes then write /set name, then enter and write username");
            topWrite = Console.CursorTop;
        }

        private static void RequestLoop()
        {
           
            string requestSent = string.Empty;

            Thread listenThread = new Thread(new ThreadStart(ReceiveResponse));
            // Start thread  
            listenThread.Start();

            try
            {
                while (requestSent.ToLower() != "/exit")
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Write(new string(' ', Console.BufferWidth));
                    Console.SetCursorPosition(0, 0);
                    Console.Write("<{0}> (you): ", username);
                    
                    
                    requestSent = Console.ReadLine();
                    string formattedRequest = username + "@" + requestSent;
                    ClientSocket.Send(Encoding.UTF8.GetBytes(formattedRequest), SocketFlags.None);

                }
                _closing = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error! Lost server");
                topWrite++;
                Console.WriteLine(e);
                topWrite++;
                Console.ReadLine();

            }
        }
        private static void ReceiveResponse()
        {
           
            while (!_closing)
            {
                var buffer = new byte[2048];
                int received = ClientSocket.Receive(buffer, SocketFlags.None);
                if (received == 0)
                    return;
                leftRead = Console.CursorLeft;
                
                
                string receivedText = Encoding.UTF8.GetString(buffer, 0, received);
                
                List<string> arrTexttypeText = receivedText.Split('@').ToList<string>();
                string TextType = "";
                string text = "";
 
                
                for (int index = 0; index < arrTexttypeText.Count - 1; index = index + 2)
                {
                    TextType = arrTexttypeText[index];
                    text = arrTexttypeText[index + 1];
                    switch (TextType)
                    {

                        case "setName":
                            username = text;
                            Console.SetCursorPosition(0, 0);
                            Console.Write(new string(' ', Console.BufferWidth));
                            Console.SetCursorPosition(0, 0);
                            string newWritePrompt = "<" + username + "> (you): ";
                            Console.Write(newWritePrompt);
                            leftRead = newWritePrompt.Length;
                            break;

                        default:
                            Console.SetCursorPosition(0, topWrite);
                            Console.WriteLine(TextType + ": " + text);

                            topWrite++;
                            break;
                    }
                }
                 
                
                
                
                

                Console.SetCursorPosition(leftRead, 0);
            }

        }
    }
}
