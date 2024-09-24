using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
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
            get
            {
                return messageList;
            }
            set
            {
                Set<ObservableCollection<RequestModel>>(ref messageList, value);

            }
        }

        public TcpClient Client { get; set; }
        public StreamWriter streamWriter { get; set; }

        public StreamReader streamReader { get; set; }


        private RelayCommand connectCommand;

        public RelayCommand ConnectCommand
        {
            get
            {
                return connectCommand ?? new RelayCommand(
                  () =>
                  {
                      IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

                      Client = new TcpClient();

                      Client.Connect(iep);

                      streamWriter = new StreamWriter(Client.GetStream());
                      streamReader = new StreamReader(Client.GetStream());

                      var json = JsonSerializer.Serialize<RequestModel>(new RequestModel { Username = Username });
                      streamWriter.WriteLine(json);

                      streamWriter.Flush();

                      ListenStream();

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

                      var json = JsonSerializer.Serialize<RequestModel>(new RequestModel { Username = Username, Message = Message });
                      streamWriter.WriteLine(json);

                      streamWriter.Flush();

                      Message = string.Empty;
                  });
            }

        }


        public void ListenStream()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var jsonData = streamReader.ReadLine();

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
