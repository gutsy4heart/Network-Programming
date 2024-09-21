using System.Net.Sockets;
using System.Net;
using System.Text;

public class TimeClient
{
    public static void Main(string[] args)
    {
        TCMethod();
    }

    public static void TCMethod()
    {
        while (true)
        {
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                clientSocket.Connect(serverEndPoint);

                Console.WriteLine("Введите запрос (time или date): ");
                string request = Console.ReadLine();

                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                clientSocket.Send(requestBytes);

                byte[] buffer = new byte[1024];
                int receivedBytes = clientSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                Console.WriteLine($"Ответ от сервера: {response}");

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}