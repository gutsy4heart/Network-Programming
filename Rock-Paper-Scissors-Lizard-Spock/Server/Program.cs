using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

class GameServer
{
    private static TcpListener listener;
    private const int port = 5000;

    public static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine("Server is running... Waiting for connections.");

        while (true)
        {
            TcpClient client1 = listener.AcceptTcpClient();
            TcpClient client2 = listener.AcceptTcpClient();
            Console.WriteLine("Two players connected. Starting game...");

            GameSession session = new GameSession(client1, client2);
            session.StartGame();
        }
    }
}

class GameSession
{
    private TcpClient player1;
    private TcpClient player2;
    private StreamReader reader1;
    private StreamWriter writer1;
    private StreamReader reader2;
    private StreamWriter writer2;
    private int player1Score;
    private int player2Score;
    private const int maxRounds = 5;
    private int currentRound = 0;

    public GameSession(TcpClient player1, TcpClient player2)
    {
        this.player1 = player1;
        this.player2 = player2;

        reader1 = new StreamReader(player1.GetStream());
        writer1 = new StreamWriter(player1.GetStream()) { AutoFlush = true };
        reader2 = new StreamReader(player2.GetStream());
        writer2 = new StreamWriter(player2.GetStream()) { AutoFlush = true };

        this.player1Score = 0;
        this.player2Score = 0;
    }

    public void StartGame()
    {
        while (currentRound < maxRounds)
        {
            string move1 = ReceiveMove(reader1);
            string move2 = ReceiveMove(reader2);

            if (move1 == "Draw" || move2 == "Draw")
            {
                if (move1 == "Draw" && move2 == "Draw")
                {
                    SendMessage(writer1, "Game ends in a draw.");
                    SendMessage(writer2, "Game ends in a draw.");
                    break;
                }
                else
                {
                    SendMessage(writer1, "Draw offer rejected.");
                    SendMessage(writer2, "Draw offer rejected.");
                }
            }
            else if (move1 == "Concede" || move2 == "Concede")
            {
                if (move1 == "Concede")
                {
                    SendMessage(writer1, "You conceded. Player 2 wins.");
                    SendMessage(writer2, "Player 1 conceded. You win.");
                }
                else
                {
                    SendMessage(writer1, "Player 2 conceded. You win.");
                    SendMessage(writer2, "You conceded. Player 1 wins.");
                }
                break;
            }
            else
            {
                string result = DetermineWinner(move1, move2);
                SendMessage(writer1, result);
                SendMessage(writer2, result);

                UpdateScores(result);
                currentRound++;
            }
        }

        string finalResult = GetFinalResult();
        SendMessage(writer1, finalResult);
        SendMessage(writer2, finalResult);

        player1.Close();
        player2.Close();
    }

    private string ReceiveMove(StreamReader reader)
    {
        return reader.ReadLine();
    }

    private void SendMessage(StreamWriter writer, string message)
    {
        writer.WriteLine(message);
    }

    private string DetermineWinner(string move1, string move2)
    {
        if (move1 == move2) return "Draw";

        // Камень побеждает ножницы и ящерицу
        if (move1 == "Rock" && (move2 == "Scissors" || move2 == "Lizard")) return "Player 1 wins";
        if (move2 == "Rock" && (move1 == "Scissors" || move1 == "Lizard")) return "Player 2 wins";

        // Бумага побеждает камень и Спока
        if (move1 == "Paper" && (move2 == "Rock" || move2 == "Spock")) return "Player 1 wins";
        if (move2 == "Paper" && (move1 == "Rock" || move1 == "Spock")) return "Player 2 wins";

        // Ножницы побеждают бумагу и ящерицу
        if (move1 == "Scissors" && (move2 == "Paper" || move2 == "Lizard")) return "Player 1 wins";
        if (move2 == "Scissors" && (move1 == "Paper" || move1 == "Lizard")) return "Player 2 wins";

        // Ящерица побеждает бумагу и Спока
        if (move1 == "Lizard" && (move2 == "Paper" || move2 == "Spock")) return "Player 1 wins";
        if (move2 == "Lizard" && (move1 == "Paper" || move1 == "Spock")) return "Player 2 wins";

        // Спок побеждает камень и ножницы
        if (move1 == "Spock" && (move2 == "Rock" || move2 == "Scissors")) return "Player 1 wins";
        if (move2 == "Spock" && (move1 == "Rock" || move1 == "Scissors")) return "Player 2 wins";

        return "Draw";
    }

    private void UpdateScores(string result)
    {
        if (result == "Player 1 wins") player1Score++;
        else if (result == "Player 2 wins") player2Score++;
    }

    private string GetFinalResult()
    {
        return $"Final Score - Player 1: {player1Score}, Player 2: {player2Score}";
    }
}
