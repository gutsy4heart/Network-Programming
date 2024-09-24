using System.Net.Sockets;
using System.Net;
using System.Text;
public class Program
{
    private static readonly Dictionary<string, double> exchangeRates = new Dictionary<string, double>
    {
        { "USD_AZN", 1.70 },
        { "AZN_USD", 0.59 },
        {"EURO_AZN", 1.89},
        {"AZN_EURO", 0.53},
        { "USD_EURO", 0.94 },
        { "EURO_USD", 1.06 },
        { "USD_GBP", 0.77 },
        { "GBP_USD", 1.30 }
    };

    private static readonly Dictionary<string, (int requestCount, DateTime lastRequestTime)> clientRequests = new Dictionary<string, (int, DateTime)>();
    private static readonly int maxRequests = 5; 
    private static readonly TimeSpan blockDuration = TimeSpan.FromMinutes(1); 
    private static int activeConnections = 0; 
    private static readonly int maxConnections = 3;

    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Console.WriteLine("Currency Server is running...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();

           
            if (activeConnections >= maxConnections)
            {
                NotifyServerOverload(client);
                continue; 
            }

            Interlocked.Increment(ref activeConnections); 
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        string clientEndPoint = client.Client.RemoteEndPoint.ToString();
        Console.WriteLine($"{DateTime.Now}: Client connected - {clientEndPoint}");

        if (IsClientBlocked(clientEndPoint))
        {
            using (NetworkStream stream = client.GetStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
            {
                writer.WriteLine("You are blocked. Try again in 1 minute.");
            }
            client.Close();
            Console.WriteLine($"{DateTime.Now}: Blocked client tried to connect - {clientEndPoint}");
            Interlocked.Decrement(ref activeConnections); // Уменьшаем количество активных подключений
            return;
        }

        using (NetworkStream stream = client.GetStream())
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            string request;
            while ((request = reader.ReadLine()) != null)
            {
                Console.WriteLine($"{DateTime.Now}: Received from {clientEndPoint} - {request}");

                if (HasExceededRequestLimit(clientEndPoint))
                {
                    writer.WriteLine("Request limit exceeded. You are blocked for 1 minute.");
                    Console.WriteLine($"{DateTime.Now}: Client {clientEndPoint} reached request limit and is blocked.");
                    break;
                }

                
                string[] currencies = request.Split(' ');
                if (currencies.Length == 2)
                {
                    string key = $"{currencies[0]}_{currencies[1]}";
                    if (exchangeRates.TryGetValue(key, out double rate))
                    {
                        writer.WriteLine(rate);
                        Console.WriteLine($"{DateTime.Now}: Sent to {clientEndPoint} - {rate}");
                    }
                    else
                    {
                        writer.WriteLine("Invalid currency pair");
                        Console.WriteLine($"{DateTime.Now}: Invalid pair requested by {clientEndPoint}");
                    }
                }
                else
                {
                    writer.WriteLine("Invalid request format");
                }

               
                IncrementClientRequestCount(clientEndPoint);
            }
        }

        Console.WriteLine($"{DateTime.Now}: Client disconnected - {clientEndPoint}");
        client.Close();
        Interlocked.Decrement(ref activeConnections);
    }

    private static void NotifyServerOverload(TcpClient client)
    {
        Console.WriteLine($"{DateTime.Now}: Server overloaded. Client connection refused.");
        using (NetworkStream stream = client.GetStream())
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            writer.WriteLine("Server is currently under maximum load. Please try again later.");
        }
        client.Close();
    }

    private static bool IsClientBlocked(string clientEndPoint)
    {
        if (clientRequests.ContainsKey(clientEndPoint))
        {
            var (requestCount, lastRequestTime) = clientRequests[clientEndPoint];
            if (requestCount >= maxRequests && DateTime.Now - lastRequestTime < blockDuration)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasExceededRequestLimit(string clientEndPoint)
    {
        if (clientRequests.ContainsKey(clientEndPoint))
        {
            var (requestCount, lastRequestTime) = clientRequests[clientEndPoint];
            if (requestCount >= maxRequests)
            {
                clientRequests[clientEndPoint] = (requestCount, DateTime.Now); // Обновляем время блокировки
                return true;
            }
        }
        return false;
    }

    private static void IncrementClientRequestCount(string clientEndPoint)
    {
        if (clientRequests.ContainsKey(clientEndPoint))
        {
            var (requestCount, lastRequestTime) = clientRequests[clientEndPoint];
            clientRequests[clientEndPoint] = (requestCount + 1, DateTime.Now);
        }
        else
        {
            clientRequests[clientEndPoint] = (1, DateTime.Now);
        }
    }
}