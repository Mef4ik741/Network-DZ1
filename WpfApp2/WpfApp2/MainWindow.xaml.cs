using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace TcpServerApp
{
    public partial class MainWindow : Window
    {
        private Thread serverThread;

        public MainWindow()
        {
            InitializeComponent();
            serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void StartServer()
        {
            Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new(IPAddress.Parse("127.0.0.1"), 3003);
            byte[] buffer = new byte[2048];

            try
            {
                serverSocket.Bind(endPoint);
                serverSocket.Listen();

                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();

                    while (true)
                    {
                        try
                        {
                            int bytesRead = clientSocket.Receive(buffer);

                            if (bytesRead == 0)
                            {
                                break;
                            }

                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine($"Received: {message}");

                            string response = $"Server received: {message}";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            clientSocket.Send(responseBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Receive error: {ex.Message}");
                            break;
                        }
                    }

                    try
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }
}
