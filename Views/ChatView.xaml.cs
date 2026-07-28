// ChatView.xaml.cs
// The Chat tab. Shows chat bubbles and an input box.
// Sends what the user typed into the NLP service and prints the reply.
// POE Part 2: Chat Bot GUI Design. POE Part 3: NLP front-end.
//
// References:
//   Microsoft Docs. ObservableCollection<T> Class.
//     https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1
//   GeeksforGeeks. WPF Tutorial.
//     https://www.geeksforgeeks.org/c-sharp/wpf-tutorial/

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CyberSecurityBot.Models;
using CyberSecurityBot.Services;

namespace CyberSecurityBot.Views
{
    public partial class ChatView : UserControl
    {
        // List of every chat message. ObservableCollection updates the UI for us
        // when we add to it. Reference: Microsoft Docs (link above).
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        private ServiceContainer _services;

        public ChatView()
        {
            InitializeComponent();
            ChatList.ItemsSource = Messages;
        }

        // Called once by MainWindow when the app is ready.
        public void Bind(ServiceContainer services)
        {
            _services = services;
            AppendBot(services.Chat.WelcomeMessage());
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Send(InputBox.Text);
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Pressing Enter sends the message.
            if (e.Key == Key.Enter)
            {
                Send(InputBox.Text);
                e.Handled = true;
            }
        }

        // Main method: take the user's text, ask the NLP service what to do, show the reply.
        private void Send(string raw)
        {
            if (_services == null) return;

            string text = raw == null ? "" : raw.Trim();
            if (text.Length == 0) return;

            AppendUser(text);
            InputBox.Clear();

            // Ask NLP service to figure out the user's intent and produce a reply.
            NlpResult parsed = _services.Nlp.Parse(text);
            bool showLog;
            bool startQuiz;
            bool showTasks;
            string reply = _services.Nlp.Handle(parsed, out showLog, out startQuiz, out showTasks);
            AppendBot(reply);

            if (parsed.Intent == Intent.Goodbye)
            {
                _services.ActivityLog.Log("Chat", "User said goodbye.");
            }
        }

        private void AppendUser(string text)
        {
            ChatMessage msg = new ChatMessage();
            msg.Sender = ChatSender.User;
            msg.Text = text;
            Messages.Add(msg);
            ChatScroll.ScrollToEnd();
        }

        private void AppendBot(string text)
        {
            ChatMessage msg = new ChatMessage();
            msg.Sender = ChatSender.Bot;
            msg.Text = text;
            Messages.Add(msg);
            ChatScroll.ScrollToEnd();
        }
    }
}
