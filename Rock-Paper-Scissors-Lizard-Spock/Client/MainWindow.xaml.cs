using System;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public partial class MainWindow : Window
    {
        public TcpClient client;
        public StreamReader reader;
        public StreamWriter writer;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("localhost", 5000);
                NetworkStream stream = client.GetStream();

                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                GameStatus.Text = "Connected to the server. Start the game!";
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Failed to connect to server: {ex.Message}";
            }
        }

        private void OnMakeMove(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MoveSelection.SelectedItem == null)
                {
                    GameStatus.Text += "\nPlease select a move.";
                    return;
                }

                string move = ((ComboBoxItem)MoveSelection.SelectedItem).Content.ToString();
                SendMove(move);
                string result = ReceiveMessage();

                if (result != null)
                {
                    GameStatus.Text += $"\n{result}";
                }

                if (result.Contains("Final Score") || result.Contains("wins") || result.Contains("draw"))
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error while making a move: {ex.Message}";
            }
        }

        private void OnOfferDraw(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMove("Draw");
                string result = ReceiveMessage();

                if (result != null)
                {
                    GameStatus.Text += $"\n{result}";
                }

                if (result.Contains("Final Score") || result.Contains("Game ends in a draw"))
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error while offering a draw: {ex.Message}";
            }
        }

        private void OnConcede(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMove("Concede");
                string result = ReceiveMessage();

                if (result != null)
                {
                    GameStatus.Text += $"\n{result}";
                }

                if (result.Contains("Final Score") || result.Contains("wins"))
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error while conceding: {ex.Message}";
            }
        }

        private void SendMove(string move)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    GameStatus.Text = "Not connected to the server.";
                    return;
                }

                writer.WriteLine(move);
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error sending move: {ex.Message}";
            }
        }

        private string ReceiveMessage()
        {
            try
            {
                return reader.ReadLine();
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error receiving message: {ex.Message}";
                return null;
            }
        }

        private void Disconnect()
        {
            try
            {
                reader.Close();
                writer.Close();
                client.Close();
                GameStatus.Text += "\nDisconnected from the server.";
            }
            catch (Exception ex)
            {
                GameStatus.Text = $"Error during disconnection: {ex.Message}";
            }
        }
    }
}
