using Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

public class Program
{
    public static void Main(string[] args)
    {

        List<Client> clients = new List<Client>();

        IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

        TcpListener tcpListener = new TcpListener(iep);

        tcpListener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {

            Task.Run(() =>
            {
                try
                {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    var stream = tcpClient.GetStream();

                    StreamReader reader = new StreamReader(stream);

                    RequestModel requestModel;

                    requestModel = JsonSerializer.Deserialize<RequestModel>(reader.ReadLine());

                    clients.Add(new Client { Username = requestModel.Username, TcpClient = tcpClient, StreamWriter = new StreamWriter(stream) });

                    Console.WriteLine(requestModel.Username + " connected...");

                    while (true)
                    {
                        var jsonData = reader.ReadLine();

                        requestModel = JsonSerializer.Deserialize<RequestModel>(jsonData);

                        Console.WriteLine(requestModel.Username + " : " + requestModel.Message);

                        foreach (var client in clients)
                        {
                            client.StreamWriter.WriteLine(jsonData);
                            client.StreamWriter.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });

        }
    }
}
