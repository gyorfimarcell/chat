using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient
{
    public class Message
    {
        public Message(MessageType type, string sender, string text)
        {
            Type = type;
            Sender = sender;
            Text = text;
        }

        public MessageType Type { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }
    }

    public enum MessageType
    {
        Connect,
        Disconnect,
        Public,
        UserListSync,
    }
}
