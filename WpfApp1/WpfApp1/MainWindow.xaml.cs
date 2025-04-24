using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace TcpClientApp
{
    public partial class MainWindow : Window
    {
        private Socket clientSocket;
        private Thread receiveThread;
        private bool isReceiving = true;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress serverIP = IPAddress.Parse("127.0.0.1");
                IPEndPoint serverEndPoint = new IPEndPoint(serverIP, 3003);
                clientSocket.Connect(serverEndPoint);
                MessagesListBox.Items.Add("Connected to server");

                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessagesListBox.Items.Add($"Connection error: {ex.Message}");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientSocket == null || !clientSocket.Connected) return;

            string username = UsernameTextBox.Text.Trim();
            string message = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(message)) return;

            string fullMessage = $"{username}: {message}";
            byte[] dataToSend = Encoding.UTF8.GetBytes(fullMessage);

            try
            {
                clientSocket.Send(dataToSend);
                MessagesListBox.Items.Add($"You: {message}");
                MessageTextBox.Clear();

                if (message.ToLower() == "exit")
                {
                    isReceiving = false;
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                MessagesListBox.Items.Add($"Send error: {ex.Message}");
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[2048];

            while (isReceiving)
            {
                try
                {
                    int bytesRead = clientSocket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Dispatcher.Invoke(() =>
                        {
                            MessagesListBox.Items.Add(message);
                        });
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            isReceiving = false;
            base.OnClosed(e);

            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch { }
            }

            receiveThread?.Join();
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernamePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernamePlaceholder.Visibility = Visibility.Visible;
            }
        }
    }
}