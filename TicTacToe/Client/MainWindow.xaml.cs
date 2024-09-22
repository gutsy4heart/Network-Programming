using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private string currentPlayer;
        private string playerSymbol;
        private string[,] board = new string[3, 3];
        private bool isMyTurn = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                reader = new StreamReader(client.GetStream(), Encoding.ASCII);
                writer = new StreamWriter(client.GetStream(), Encoding.ASCII) { AutoFlush = true };

                currentPlayer = UsernameBox.Text;
                writer.WriteLine(currentPlayer); // Отправляем имя игрока на сервер

                playerSymbol = reader.ReadLine();
                PlayerSymbolLabel.Text = "You are: " + playerSymbol;

                isMyTurn = playerSymbol == "X";

                Thread listenerThread = new Thread(ListenForMessages) { IsBackground = true };
                listenerThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private void ListenForMessages()
        {
            while (true)
            {
                try
                {
                    string message = reader.ReadLine();
                    if (message == null)
                    {
                        Dispatcher.Invoke(() => MessageBox.Show("Connection to server lost."));
                        break;
                    }

                    if (message.StartsWith("move"))
                    {
                        var parts = message.Split('|');
                        int row = int.Parse(parts[1]);
                        int col = int.Parse(parts[2]);
                        string symbol = parts[3];
                        Dispatcher.Invoke(() => UpdateBoard(row, col, symbol));
                    }
                    else if (message.StartsWith("nextPlayer"))
                    {
                        string nextPlayer = message.Split('|')[1];
                        Dispatcher.Invoke(() => {
                            CurrentPlayerLabel.Text = "Current Player: " + nextPlayer;
                            isMyTurn = (nextPlayer == currentPlayer);
                        });
                    }
                    else if (message.StartsWith("win"))
                    {
                        var parts = message.Split('|');
                        string winningPlayer = parts[1];
                        string winningSymbol = parts[2];
                        Dispatcher.Invoke(() => {
                            MessageBox.Show($"{winningPlayer} ({winningSymbol}) wins!");
                            ResetBoard();
                        });
                    }
                    else if (message == "draw")
                    {
                        Dispatcher.Invoke(() => {
                            MessageBox.Show("It's a draw!");
                            ResetBoard();
                        });
                    }

                }
                catch (IOException ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Connection error: " + ex.Message));
                    break;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Unexpected error: " + ex.Message));
                    break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content == null && isMyTurn)
            {
                int row = Grid.GetRow(button);
                int col = Grid.GetColumn(button);

                button.Content = playerSymbol;
                board[row, col] = playerSymbol;

                writer.WriteLine($"move|{row}|{col}|{playerSymbol}");
                isMyTurn = false; 

               
                if (CheckForWinner(row, col, playerSymbol))
                {
                    Dispatcher.Invoke(() => {
                        MessageBox.Show($"{currentPlayer} ({playerSymbol}) wins!");
                        ResetBoard();
                    });
                }
                else if (IsDraw())
                {
                    Dispatcher.Invoke(() => {
                        MessageBox.Show("It's a draw!");
                        ResetBoard();
                    });
                }
            }
        }

        private void UpdateBoard(int row, int col, string symbol)
        {
            board[row, col] = symbol;
            Button button = GetButtonAt(row, col);
            button.Content = symbol;
            CurrentPlayerLabel.Text = "Current Player: " + (symbol == "X" ? "O" : "X");
        }

        private Button GetButtonAt(int row, int col)
        {
            return (Button)this.FindName($"Button{row * 3 + col + 1}");
        }

        private void ResetBoard()
        {
            board = new string[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Button button = GetButtonAt(i, j);
                    button.Content = null;
                }
            }

            isMyTurn = playerSymbol == "X";
            CurrentPlayerLabel.Text = "Current Player: " + (isMyTurn ? currentPlayer : "Waiting...");
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

            if (writer != null)
            {
                writer.WriteLine("startGame"); 
            }

            CurrentPlayerLabel.Text = "Current Player: " + currentPlayer;
            isMyTurn = (playerSymbol == "X");
        }

        private bool CheckForWinner(int row, int col, string symbol)
        {
           
            if (board[row, 0] == symbol && board[row, 1] == symbol && board[row, 2] == symbol) return true;

            
            if (board[0, col] == symbol && board[1, col] == symbol && board[2, col] == symbol) return true;

            
            if (row == col && board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol) return true;
            if (row + col == 2 && board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol) return true;

            return false;
        }

        private bool IsDraw()
        {
            foreach (var cell in board)
            {
                if (cell == null) return false; 
            }
            return true;
        }
    }
}
