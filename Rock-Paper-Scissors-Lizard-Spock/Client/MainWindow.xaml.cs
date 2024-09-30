using System.Net.Sockets;
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
        private TcpClient client;
        private NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("localhost", 5000);
                stream = client.GetStream();
                GameStatus.Text = "Connected to the server. Start the game!";
            }
            catch (Exception ex)
            {
                GameStatus.Text = "Failed to connect to server.";
            }
        }

        private void OnMakeMove(object sender, RoutedEventArgs e)
        {
            if (MoveSelection.SelectedItem == null) return;

            string move = ((ComboBoxItem)MoveSelection.SelectedItem).Content.ToString();
            SendMove(move);
            string result = ReceiveMessage();
            GameStatus.Text += $"\n{result}";
        }

        private void OnOfferDraw(object sender, RoutedEventArgs e)
        {
            SendMove("Draw");
            string result = ReceiveMessage();
            GameStatus.Text += $"\n{result}";
        }

        private void OnConcede(object sender, RoutedEventArgs e)
        {
            SendMove("Concede");
            string result = ReceiveMessage();
            GameStatus.Text += $"\n{result}";
            Disconnect();
        }

        private void SendMove(string move)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(move);
            stream.Write(buffer, 0, buffer.Length);
        }

        private string ReceiveMessage()
        {
            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private void Disconnect()
        {
            stream.Close();
            client.Close();
        }
    }
}