using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ClientViewModel();
        }
    }

    class RequestModel
    {
        public string Username { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Username} : {Message}";
        }
    }

    class ClientViewModel : ViewModelBase
    {
        public ClientViewModel()
        {
            IP = "127.0.0.1";
            PORT = "8080";
        }

        private string ip;
        public string IP
        {
            get { return ip; }
            set { Set(ref ip, value); }
        }

        private string port;
        public string PORT
        {
            get { return port; }
            set { Set(ref port, value); }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { Set(ref username, value); }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }

        private ObservableCollection<RequestModel> messageList = new ObservableCollection<RequestModel>();

        public ObservableCollection<RequestModel> MessageList
        {
            get { return messageList; }
            set { Set(ref messageList, value); }
        }

        private Socket socket;

        private RelayCommand connectCommand;

        public RelayCommand ConnectCommand
        {
            get
            {
                return connectCommand ?? new RelayCommand(
                  () =>
                  {
                      // Создаем сокет и подключаемся к серверу
                      socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                      var endPoint = new IPEndPoint(IPAddress.Parse(IP), int.Parse(PORT));

                      // Отправляем имя пользователя на сервер
                      var json = JsonSerializer.Serialize(new RequestModel { Username = Username });
                      var data = Encoding.UTF8.GetBytes(json);
                      socket.SendTo(data, endPoint);

                      ListenForMessages();
                  });
            }
        }

        private RelayCommand sendMessageCommand;

        public RelayCommand SendMessageCommand
        {
            get
            {
                return sendMessageCommand ?? new RelayCommand(
                  () =>
                  {
                      if (Message == "Bye")
                      {
                          // Отправляем сообщение о выходе и закрываем сокет
                          var json = JsonSerializer.Serialize(new RequestModel { Username = Username, Message = "Bye" });
                          var data = Encoding.UTF8.GetBytes(json);
                          var endPoint = new IPEndPoint(IPAddress.Parse(IP), int.Parse(PORT));

                          socket.SendTo(data, endPoint);
                          socket.Close(); // Закрываем сокет
                          Application.Current.Shutdown(); // Завершаем приложение
                          return; // Прерываем дальнейшую обработку
                      }

                      var jsonMessage = JsonSerializer.Serialize(new RequestModel { Username = Username, Message = Message });
                      var messageData = Encoding.UTF8.GetBytes(jsonMessage);
                      var sendEndPoint = new IPEndPoint(IPAddress.Parse(IP), int.Parse(PORT));

                      // Отправляем сообщение на сервер
                      socket.SendTo(messageData, sendEndPoint);
                      Message = string.Empty;
                  });
            }
        }

        public void ListenForMessages()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                    string jsonData = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    var requestModel = JsonSerializer.Deserialize<RequestModel>(jsonData);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageList.Add(requestModel);
                    });
                }
            });
        }
    }
}
