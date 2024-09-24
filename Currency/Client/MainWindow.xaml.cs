using System.IO;
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
        private StreamWriter writer;
        private StreamReader reader;

        public MainWindow()
        {
            InitializeComponent();

            try
            {

                client = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = client.GetStream();
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                reader = new StreamReader(stream, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to server: " + ex.Message);
            }
        }

        private void OnGetRateClick(object sender, RoutedEventArgs e)
        {
            string currencyPair = CurrencyPairInput.Text;

            if (!string.IsNullOrWhiteSpace(currencyPair))
            {
                try
                {

                    writer.WriteLine(currencyPair);
                    string response = reader.ReadLine();
                    ResponseText.Text = "Exchange rate: " + response;
                }
                catch (Exception ex)
                {
                    ResponseText.Text = "Error: " + ex.Message;
                }
            }
            else
            {
                ResponseText.Text = "Please enter a valid currency pair.";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (client != null)
            {
                writer.Close();
                reader.Close();
                client.Close();
            }
        }
    }
}