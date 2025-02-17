﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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

        ObservableCollection<string> users = [];

        public ChatWindow(string username)
        {
            this.username = username;

            InitializeComponent();

            lbUsers.ItemsSource = users;

            Connect();
        }

        public async Task Connect()
        {
            client = new();
            await client.ConnectAsync(new Uri("ws://localhost:7890"), CancellationToken.None);
            await SendMessage(new Message(MessageType.Connect, username, "", null));

            Title = $"Chat - {username}";

            await RecieveMessages();
        }

        public async Task SendMessage(Message message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToJson()));
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task RecieveMessages()
        {
            while (client.State == WebSocketState.Open)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await client.ReceiveAsync(buffer, CancellationToken.None);
                string jsonMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                Message? message = JsonSerializer.Deserialize<Message>(jsonMessage, options: new() {PropertyNameCaseInsensitive = true });
                if (message != null)
                {
                    switch (message.Type)
                    {
                        case MessageType.Connect:
                            lbMessages.Items.Add($"System: {message.Sender} connected.");
                            users.Add(message.Sender);
                            UpdateUsers();
                            break;
                        case MessageType.Disconnect:
                            lbMessages.Items.Add($"System: {message.Sender} disconnected.");
                            users.Remove(message.Sender);
                            UpdateUsers();
                            break;
                        case MessageType.Public:
                            lbMessages.Items.Add($"{message.Sender}: {message.Text}");
                            break;
                        case MessageType.UserListSync:
                            users = new(JsonSerializer.Deserialize<List<string>>(message.Text));
                            UpdateUsers();
                            break;
                        case MessageType.Private:
                            lbMessages.Items.Add($">{message.Sender}>{message.Target}: {message.Text}");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void UpdateUsers() {
            lbUsers.ItemsSource = users;
            cbTarget.ItemsSource = users.Where(x => x != username).Prepend("Public");

            if (cbTarget.SelectedIndex == -1)
            {
                cbTarget.SelectedIndex = 0;
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text != "")
            {
                if (cbTarget.SelectedIndex == 0)
                {
                    await SendMessage(new Message(MessageType.Public, username, tbMessage.Text, null));
                }
                else
                {
                    await SendMessage(new Message(MessageType.Private, username, tbMessage.Text, cbTarget.SelectedItem.ToString()));
                }
     
                tbMessage.Clear();
            }
        }
    }
}
