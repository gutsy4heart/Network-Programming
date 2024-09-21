using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket socket;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse("127.0.0.1");
                    IPEndPoint iep = new IPEndPoint(ip, 8080);
                    socket.Bind(iep);
                    socket.Listen(10);

                    Dispatcher.Invoke(() => MessagesList.Items.Add(DateTime.Now.ToString() + "  Waiting for connections..."));

                    while (true)
                    {
                        var clientSocket = socket.Accept();
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                byte[] buffer = new byte[1024];
                                int dataSize = clientSocket.Receive(buffer);
                                string data = Encoding.ASCII.GetString(buffer, 0, dataSize);

                                Dispatcher.Invoke(() => MessagesList.Items.Add(DateTime.Now.ToString() + "  " + data));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessagesList.Items.Add("Server Error: " + ex.Message));
                }
            });
        }
    }
}