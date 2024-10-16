using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        string username;
        ClientWebSocket client;

        public ChatWindow(string username)
        {
            this.username = username;

            InitializeComponent();

            Connect();
        }

        public async Task Connect()
        {
            client = new();
            await client.ConnectAsync(new Uri("ws://localhost:7890"), CancellationToken.None);
            await SendMessage($"[connect]{username}");
            await RecieveMessages();
        }

        public async Task SendMessage(string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task RecieveMessages()
        {
            while (client.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await client.ReceiveAsync(buffer, CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                lbMessages.Items.Add(message);
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text != "")
            {
                await SendMessage($"[public]{tbMessage.Text}");
                tbMessage.Clear();
            }
        }
    }
}
