// ChatMessage.cs
// One message in the chat - either from the user or from the bot.
// IsUser/IsBot are used by the XAML to pick which bubble style to draw.

using System;

namespace CyberSecurityBot.Models
{
    public enum ChatSender { User, Bot }

    public class ChatMessage
    {
        public ChatSender Sender { get; set; }
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Helper properties used by the chat XAML to show the right bubble style.
        public bool IsUser { get { return Sender == ChatSender.User; } }
        public bool IsBot { get { return Sender == ChatSender.Bot; } }
    }
}
