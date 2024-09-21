using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq.Expressions;

public class TimeServer
{
    public static void Main(string[] args)
    {
        TSMethod();
    }
    public static void TSMethod()
    {
        try
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            server.Bind(ep);

            server.Listen(10);
            Console.WriteLine("Сервер запущен. Ожидание подключения клиентов...");

            
            while (true)
            {
                Socket clientSocket = server.Accept();
                Console.WriteLine("Клиент подключен.");

                byte[] buffer = new byte[1024];
                int receivedBytes = clientSocket.Receive(buffer);
                string request = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                string response = "";
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
                Console.WriteLine($"Отправлен ответ клиенту: {response}");

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}