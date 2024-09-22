using System.Net;
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

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.IsBackground = true; // Сервер будет работать в фоновом потоке
            serverThread.Start();
        }

        private void StartServer()
        {
            try
            {
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                server.Bind(ep);

                server.Listen(10);
                Dispatcher.Invoke(() => MessagesList.Items.Add("Сервер запущен. Ожидание подключения клиентов..."));

                while (true)
                {
                    Socket clientSocket = server.Accept();
                    Dispatcher.Invoke(() => MessagesList.Items.Add("Клиент подключен."));

                    byte[] buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
                    string request = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    string response;
                    if (request.ToLower() == "time")
                    {
                        response = DateTime.Now.ToString("HH:mm:ss");
                    }
                    else if (request.ToLower() == "date")
                    {
                        response = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        response = "Invalid request";
                    }

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    clientSocket.Send(responseBytes);

                    Dispatcher.Invoke(() => MessagesList.Items.Add($"Получен запрос: {request}"));
                    Dispatcher.Invoke(() => MessagesList.Items.Add($"Отправлен ответ: {response}"));

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessagesList.Items.Add($"Ошибка: {ex.Message}"));
            }
        }
    }
}