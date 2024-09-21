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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket clientSocket;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clientSocket == null || !clientSocket.Connected)
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse("127.0.0.1");
                    IPEndPoint iep = new IPEndPoint(ip, 8080);
                    clientSocket.Connect(iep);
                }

                string message = MessageInput.Text;
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                clientSocket.Send(buffer);

                MessagesList.Items.Add(DateTime.Now.ToString() + "  Sent: " + message);
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                MessagesList.Items.Add("Client Error: " + ex.Message);
            }
        }
    }
}