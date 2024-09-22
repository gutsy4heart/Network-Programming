using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    private static TcpListener listener;
    private static TcpClient[] players = new TcpClient[2];
    private static StreamReader[] readers = new StreamReader[2];
    private static StreamWriter[] writers = new StreamWriter[2];
    private static int currentPlayer = 0;
    private static string[] playerSymbols = { "X", "O" };
    private static string[] playerNames = new string[2];

    public static async Task Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();
        Console.WriteLine("Server started, waiting for players...");

      
        for (int i = 0; i < 2; i++)
        {
            players[i] = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"Player {i + 1} connected.");

            readers[i] = new StreamReader(players[i].GetStream(), Encoding.ASCII);
            writers[i] = new StreamWriter(players[i].GetStream(), Encoding.ASCII) { AutoFlush = true };

            
            playerNames[i] = await readers[i].ReadLineAsync();
            writers[i].WriteLine(playerSymbols[i]); 
        }

        
        BroadcastMessage("Start Game");

        while (true)
        {
            try
            {
                string message = await readers[currentPlayer].ReadLineAsync();
                if (message == null) break; 

                if (message == "startGame")
                {
                    BroadcastMessage("nextPlayer|" + playerNames[currentPlayer]);
                    currentPlayer = (currentPlayer + 1) % 2; 
                }
                else if (message.StartsWith("move"))
                {
                    var parts = message.Split('|');
                    int row = int.Parse(parts[1]);
                    int col = int.Parse(parts[2]);
                    string symbol = parts[3];
                    BroadcastMessage($"move|{row}|{col}|{symbol}");
                    
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Connection error with {playerNames[currentPlayer]}: {ex.Message}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                break;
            }
        }

        
        foreach (var player in players)
        {
            player.Close();
        }
        listener.Stop();
    }

    private static void BroadcastMessage(string message)
    {
        foreach (var writer in writers)
        {
            if (writer != null)
            {
                writer.WriteLine(message);
            }
        }
    }
}
