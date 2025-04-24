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
                AppendLog($"Listening on {endPoint.Address}:{endPoint.Port}");

                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                    AppendLog($"Client connected: {clientSocket.RemoteEndPoint}");

                    while (true)
                    {
                        try
                        {
                            int bytesRead = clientSocket.Receive(buffer);
                            AppendLog($"Bytes read: {bytesRead}");  // Добавлено логирование

                            if (bytesRead == 0)
                            {
                                AppendLog("Client disconnected.");
                                break;
                            }

                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            AppendLog($"Received: {message}");

                            string response = $"Server received: {message}";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            clientSocket.Send(responseBytes);
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"Receive error: {ex.Message}");
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
                AppendLog($"Server error: {ex.Message}");
            }
        }


        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{message}\n");
                LogTextBox.ScrollToEnd();
            });
        }
    }
}
