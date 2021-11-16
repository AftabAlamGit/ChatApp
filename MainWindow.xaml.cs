using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPEndPoint remoteIPEP;

        private TcpClient tcpClient;

        private TcpListener server;

        private NetworkStream netStream;

        private bool isServerStart;

        private bool isConnect;

        private Byte[] bytes;
        public object _lock = new object();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnect)
            {
                isConnect = true;
                buttonConnect.Content = "Disconnect";
                //Connect to remote IP
                remoteIPEP = new IPEndPoint(IPAddress.Parse(textBoxRemoteIP.Text), Convert.ToInt32(textBoxRemotePort.Text));
                tcpClient = new TcpClient();
                try
                {
                    tcpClient.Connect(remoteIPEP);


                    if (tcpClient.Client.Connected)
                    {
                        addData("Connected");
                    }

                    getMessageAsync();
                }
                catch (SocketException ex)
                {
                    addData(" " + ex.Message);
                }

            }
            else
            {
                isConnect = false;
                buttonConnect.Content = "Connect";
                tcpClient.Close();
            }

            //Connect();
        }
        private void DataRecieved(IAsyncResult ar)
        {
            bool part1 = tcpClient.Client.Poll(1000, SelectMode.SelectRead);
            bool part2 = (tcpClient.Client.Available == 0);
            
            if (part1 && part2)
            {
                netStream.Close();
                addData("Server Disconnected");
                return;
            }

            try
            {
                // Call EndRead.
                int bytesRead = netStream.EndRead(ar);
            }
            catch (Exception e)
            {
                addData("Connection Lost");
            }
            // Process the bytes here.
            String data = null;
            data = System.Text.Encoding.ASCII.GetString(bytes);
            // Console.WriteLine("Received: {0}", data);
            addData(data);

            // Determine if you want to read again.  If not, return.


            // Read again.  This callback will be called again.
            netStream.BeginRead(bytes, 0, bytes.Length, DataRecieved, null);

        }

        private void getMessageAsync()
        {
            netStream = tcpClient.GetStream();
            var bufSize = tcpClient.ReceiveBufferSize;
            bytes = new byte[bufSize];

            // Trigger the initial read.
            netStream.BeginRead(bytes, 0, bytes.Length, DataRecieved, null);
        }
        // Get the IP Address of Local machine
        static string GetLocalIP()
        {

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";

        }
        // Method for adding data to listBox from another thread
        private void addData(string data)
        {
            this.listBoxMessage.Dispatcher.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                this.listBoxMessage.Items.Add(data);
            });
        }

        private void buttonStartServer_Click(object sender, RoutedEventArgs e)
        {
            if (!isServerStart)
            {
                Task.Run(() => StartServer());
                buttonStartListening.Content = "Stop Server";
                isServerStart = true;
            }
            else
            {
                buttonStartListening.Content = "Start Server";
                isServerStart = false;
                tcpClient.Close();
                netStream.Close();
                server.Stop();
            }
        }

        private void StartServer()
        {
            server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 9000;
                var localIP = GetLocalIP();
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    tcpClient = server.AcceptTcpClient();

                    addData("Connected");

                    data = null;

                    // Get a stream object for reading and writing
                    netStream = tcpClient.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = netStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        addData(data);
                    }
                }
                
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Shutdown and end connection
                tcpClient.Close();
                // Stop listening for new clients.
                server.Stop();
            }
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            sendMessage();
        }

        private void sendMessage()
        {
            try
            {
                if (netStream.CanWrite)
                {
                    var message = textBoxMessage.Text;
                    var messageDisplay = GetLocalIP() + ": " + message;
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(messageDisplay);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                    addData(messageDisplay);
                }
                else
                {
                    //Console.WriteLine("You cannot write data to this stream.");
                    tcpClient.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    netStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                addData("Connection Lost");
            }
        }
    }
}
