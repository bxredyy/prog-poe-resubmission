// CommandService.cs
// Handles the slash commands the user can type in the chat:
// /help, /categories, /start, /quiz, /score, /tips, /about.
// Built so the chat feels more like a real assistant and less like a search box.
// POE Part 3 Task 3: NLP Flow.
//
// References:
//   GeeksforGeeks. C# Switch Statement.
//     https://www.geeksforgeeks.org/c-sharp/c-sharp-switch-statement/

using System.Text;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class CommandService
    {
        private readonly TopicCatalogService _catalog;
        private readonly ConversationStateMachine _state;
        private readonly ChatService _chat;
        private readonly ActivityLogger _log;
        private readonly UserProfile _profile;

        public CommandService(
            TopicCatalogService catalog,
            ConversationStateMachine state,
            ChatService chat,
            ActivityLogger log,
            UserProfile profile)
        {
            _catalog = catalog;
            _state = state;
            _chat = chat;
            _log = log;
            _profile = profile;
        }

        // Runs the slash command and returns the bot's reply.
        // The two "out" booleans tell the chat view if it should jump to another tab.
        public string Execute(ChatCommand cmd, out bool requestStartQuiz, out bool requestShowLog)
        {
            requestStartQuiz = false;
            requestShowLog = false;
            _log.Log("Command", "User invoked " + cmd.ToString().ToLower() + ".");

            // Simple switch - one case per command.
            switch (cmd)
            {
                case ChatCommand.Help:
                    return BuildHelp();
                case ChatCommand.Categories:
                    return _catalog.FormatCategoryList();
                case ChatCommand.Start:
                    return Reset();
                case ChatCommand.Quiz:
                    return StartQuiz(out requestStartQuiz);
                case ChatCommand.Score:
                    return BuildScore();
                case ChatCommand.Tips:
                    return _chat.PickRandomTip();
                case ChatCommand.About:
                    return BuildAbout();
                default:
                    return Unknown();
            }
        }

        public string BuildHelp()
        {
            return
                "Available commands:\n" +
                "  /help        - Show available commands\n" +
                "  /categories  - View cybersecurity topics\n" +
                "  /start       - Restart the conversation\n" +
                "  /quiz        - Start the cybersecurity quiz\n" +
                "  /score       - View your quiz progress\n" +
                "  /tips        - Get a random security recommendation\n" +
                "  /about       - Learn about this chatbot\n\n" +
                "You can also just chat: 'tell me about phishing', 'add a task to enable 2FA', " +
                "or 'remind me to update my password in 3 days'.";
        }

        // /start - reset everything and show the welcome message again.
        private string Reset()
        {
            _state.Reset();
            return "Conversation reset.\n\n" + _chat.WelcomeMessage();
        }

        // /quiz - flip to the Quiz tab and tell the user to take it.
        private string StartQuiz(out bool requestStartQuiz)
        {
            requestStartQuiz = true;
            _state.EnterChatQuiz();
            return "Starting the cybersecurity quiz - I've opened the Quiz tab for you. " +
                   "Type /score anytime to see your latest result, or /categories to come back here.";
        }

        // /score - print the user's most recent quiz attempt and how many they've done.
        private string BuildScore()
        {
            if (_state.QuizAttempts == 0)
                return "You haven't finished a quiz yet. Type /quiz to start one!";

            int pct;
            if (_state.LastQuizTotal == 0)
                pct = 0;
            else
                pct = (int)(100.0 * _state.LastQuizScore / _state.LastQuizTotal);

            // Simple if/else for the verdict.
            string verdict;
            if (pct >= 80)
                verdict = "Excellent - you really know your stuff!";
            else if (pct >= 50)
                verdict = "Solid effort. Keep practising and you'll be a pro.";
            else
                verdict = "Keep learning - type /categories to brush up on a topic.";

            return
                "Quiz progress:\n" +
                "  Latest score: " + _state.LastQuizScore + "/" + _state.LastQuizTotal +
                " (" + pct + "%)\n" +
                "  Attempts so far: " + _state.QuizAttempts + "\n" +
                verdict;
        }

        // /about - short paragraph about what the bot is.
        private string BuildAbout()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cybersecurity Awareness Bot v3.0 - PROG6221 POE 2026 (Group 04)");
            sb.AppendLine("I help South African citizens learn safe online habits through:");
            sb.AppendLine("  - guided cybersecurity topics");
            sb.AppendLine("  - a multi-question quiz");
            sb.AppendLine("  - a task assistant backed by MySQL");
            sb.AppendLine("  - an activity log of everything we discuss");
            sb.Append("Type /help to see what I can do.");
            return sb.ToString();
        }

        public string Unknown()
        {
            return "I didn't recognise that command. Try /help to see the full list, " +
                   "or /categories to browse topics. You can also just type a question " +
                   "like 'how do I avoid phishing?'.";
        }
    }
}
