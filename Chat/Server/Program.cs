using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class RequestModel
    {
        public string Username { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Username}: {Message}";
        }
    }

    class ClientInfo
    {
        public EndPoint EndPoint { get; set; }
        public string Username { get; set; }
    }

    class Program
    {
        static List<ClientInfo> clients = new List<ClientInfo>();
        static Socket socket;

        static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            Console.WriteLine("Server started...");

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        static void ReceiveMessages()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                string jsonData = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                var requestModel = JsonSerializer.Deserialize<RequestModel>(jsonData);

                // Проверяем, подключен ли клиент
                if (!clients.Exists(c => c.EndPoint.Equals(remoteEndPoint)))
                {
                    clients.Add(new ClientInfo { EndPoint = remoteEndPoint, Username = requestModel.Username });
                    Console.WriteLine($"{requestModel.Username} connected...");
                }

                Console.WriteLine(requestModel);
                if (requestModel.Message == "Bye")
                {
                    Console.WriteLine($"{requestModel.Username} disconnected.");
                    clients.RemoveAll(c => c.EndPoint.Equals(remoteEndPoint));
                }

                // Отправка сообщения всем клиентам
                foreach (var client in clients)
                {
                    var responseMessage = JsonSerializer.Serialize(requestModel);
                    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    socket.SendTo(responseBytes, client.EndPoint);
                }
            }
        }
    }
}
