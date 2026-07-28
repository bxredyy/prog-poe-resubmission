// NlpService.cs
// Reads the user's message and tries to figure out what they want (their "intent").
// Uses regex + simple keyword checks - no real AI, just string matching like the brief says.
// POE Part 3 Task 3: NLP Simulation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public enum Intent
    {
        Unknown,
        Greet,
        ProvideName,
        Help,
        Command,
        SelectMenuItem,
        AddTask,
        SetReminder,
        ShowTasks,
        CompleteTask,
        StartQuiz,
        ShowLog,
        TellMeMore,
        TopicQuestion,
        SmallTalk,
        Sentiment,
        Goodbye
    }

    public class NlpResult
    {
        public Intent Intent { get; set; }
        public string? Payload { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string? Topic { get; set; }
        public ChatCommand Command { get; set; } = ChatCommand.None;
        public int MenuIndex { get; set; }
    }

    public class NlpService
    {
        private readonly UserProfile _profile;
        private readonly TaskService _tasks;
        private readonly QuizService _quiz;
        private readonly ChatService _chat;
        private readonly ActivityLogger _log;
        private readonly TopicCatalogService _catalog;
        private readonly ConversationStateMachine _stateMachine;
        private readonly CommandService _commands;

        private static readonly Regex AddTaskRegex = new(
            @"(?:add|create|new|set up)\s+(?:a\s+)?(?:task|reminder)\s*[:\-]?\s*(?:to\s+)?(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RemindMeRegex = new(
            @"remind\s+me\s+(?:to\s+)?(.+?)(?:\s+(?:in|on|at|tomorrow|today)\s+.+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NameRegex = new(
            @"(?:my\s+name\s+is|i\s*am|i'm|call\s+me)\s+([A-Za-z][A-Za-z\-']{1,30})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InDaysRegex = new(
            @"in\s+(\d+)\s+day", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex JustNumberRegex = new(
            @"^\s*(\d{1,2})\s*[.)]?\s*$", RegexOptions.Compiled);

        public NlpService(
            UserProfile profile,
            TaskService tasks,
            QuizService quiz,
            ChatService chat,
            ActivityLogger log,
            TopicCatalogService catalog,
            ConversationStateMachine stateMachine,
            CommandService commands)
        {
            _profile = profile;
            _tasks = tasks;
            _quiz = quiz;
            _chat = chat;
            _log = log;
            _catalog = catalog;
            _stateMachine = stateMachine;
            _commands = commands;
        }

        public NlpResult Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new NlpResult { Intent = Intent.Unknown };

            var input = raw.Trim();
            var lower = input.ToLowerInvariant();

            if (ChatCommandParser.LooksLikeCommand(input))
            {
                var cmd = ChatCommandParser.Parse(input);
                return new NlpResult { Intent = Intent.Command, Command = cmd, Payload = input };
            }

            var numberMatch = JustNumberRegex.Match(input);
            if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out var n))
            {
                return new NlpResult { Intent = Intent.SelectMenuItem, MenuIndex = n };
            }

            if (lower == "exit" || lower == "quit" || lower == "bye" || lower == "goodbye")
                return new NlpResult { Intent = Intent.Goodbye };

            var nameMatch = NameRegex.Match(input);
            if (nameMatch.Success && string.IsNullOrWhiteSpace(_profile.Name))
                return new NlpResult { Intent = Intent.ProvideName, Payload = nameMatch.Groups[1].Value };

            if (lower.Contains("help") || lower.Contains("what can") || lower.Contains("how do i"))
                return new NlpResult { Intent = Intent.Help };

            if (lower.StartsWith("hi") || lower.StartsWith("hello") || lower.StartsWith("hey") || lower.Contains("good morning") || lower.Contains("good evening"))
                return new NlpResult { Intent = Intent.Greet };

            if (lower.Contains("activity log") || lower.Contains("show log") || lower.Contains("what have you done") || lower.Contains("history"))
                return new NlpResult { Intent = Intent.ShowLog };

            if (lower.Contains("quiz") || lower.Contains("test me") || lower.Contains("mini game") || lower.Contains("game"))
                return new NlpResult { Intent = Intent.StartQuiz };

            if (lower.Contains("show tasks") || lower.Contains("list tasks") || lower.Contains("my tasks") || lower.Contains("view tasks"))
                return new NlpResult { Intent = Intent.ShowTasks };

            if ((lower.Contains("done") || lower.Contains("completed") || lower.Contains("mark")) && lower.Contains("task"))
                return new NlpResult { Intent = Intent.CompleteTask, Payload = input };

            var remind = RemindMeRegex.Match(input);
            if (remind.Success)
            {
                var date = ParseDate(lower);
                return new NlpResult { Intent = Intent.SetReminder, Payload = CleanPayload(remind.Groups[1].Value), ReminderDate = date };
            }

            var addTask = AddTaskRegex.Match(input);
            if (addTask.Success)
            {
                var date = ParseDate(lower);
                return new NlpResult { Intent = Intent.AddTask, Payload = CleanPayload(addTask.Groups[1].Value), ReminderDate = date };
            }

            if (lower.Contains("another") || lower.Contains("tell me more") || lower.Contains("more on") || lower.Contains("explain more") || lower.Contains("more tips"))
                return new NlpResult { Intent = Intent.TellMeMore };

            var topic = _catalog.FindByInput(lower) ?? FindLegacyTopic(lower);
            if (topic != null)
                return new NlpResult { Intent = Intent.TopicQuestion, Topic = topic.Slug };

            if (ContainsSentiment(lower))
                return new NlpResult { Intent = Intent.Sentiment, Payload = input };

            if (IsSmallTalk(lower))
                return new NlpResult { Intent = Intent.SmallTalk, Payload = input };

            return new NlpResult { Intent = Intent.Unknown, Payload = input };
        }

        public string Handle(NlpResult result, out bool requestShowLog, out bool requestStartQuiz, out bool requestShowTasks)
        {
            requestShowLog = false;
            requestStartQuiz = false;
            requestShowTasks = false;

            switch (result.Intent)
            {
                case Intent.Command:
                    return _commands.Execute(result.Command, out requestStartQuiz, out requestShowLog);

                case Intent.SelectMenuItem:
                    return HandleMenuSelection(result.MenuIndex);

                case Intent.Greet:
                    return _chat.Greet();

                case Intent.ProvideName:
                    _profile.Name = result.Payload ?? string.Empty;
                    _log.Log("Memory", $"Remembered user name: {_profile.Name}.");
                    return $"Nice to meet you, {_profile.Name}! What would you like to learn about today? Type /categories to see the topics I cover.";

                case Intent.Help:
                    return _commands.BuildHelp();

                case Intent.AddTask:
                {
                    var title = result.Payload ?? "Untitled task";
                    var task = _tasks.Add(title, string.Empty, result.ReminderDate);
                    var reminderText = task.ReminderAt.HasValue
                        ? $" I'll remind you on {task.ReminderAt:yyyy-MM-dd}."
                        : " Want a reminder? Say something like 'remind me in 3 days'.";
                    return $"Task added: '{task.Title}'.{reminderText}";
                }

                case Intent.SetReminder:
                {
                    var title = result.Payload ?? "Reminder";
                    var date = result.ReminderDate ?? DateTime.Today.AddDays(1);
                    var task = _tasks.Add(title, "Created from a chat reminder.", date);
                    return $"Reminder set for '{task.Title}' on {date:yyyy-MM-dd}.";
                }

                case Intent.ShowTasks:
                    requestShowTasks = true;
                    if (_tasks.Tasks.Count == 0)
                        return "You don't have any tasks yet. Try 'add task to review privacy settings'.";
                    var summary = string.Join("\n", _tasks.Tasks.Take(5).Select((t, i) => $"  {i + 1}. {t.Title} - {t.StatusLabel}{(t.ReminderAt.HasValue ? $" (reminder {t.ReminderAt:yyyy-MM-dd})" : string.Empty)}"));
                    return $"Your most recent tasks:\n{summary}\n(Open the Tasks tab to manage them.)";

                case Intent.CompleteTask:
                {
                    var task = _tasks.Tasks.FirstOrDefault(t => result.Payload != null && result.Payload.Contains(t.Title, StringComparison.OrdinalIgnoreCase));
                    if (task == null) return "I couldn't find that task. Try the Tasks tab to mark it manually.";
                    _tasks.MarkDone(task);
                    return $"Nice work - marked '{task.Title}' as completed.";
                }

                case Intent.StartQuiz:
                    requestStartQuiz = true;
                    _stateMachine.EnterChatQuiz();
                    return "Starting the quiz - head to the Quiz tab and answer the questions one by one. Type /score afterwards to see how you did.";

                case Intent.ShowLog:
                    requestShowLog = true;
                    if (_log.RecentView.Count == 0) return "I haven't done anything noteworthy yet.";
                    var entries = string.Join("\n", _log.RecentView.Take(_log.DisplayLimit).Select((e, i) => $"  {i + 1}. {e.Display}"));
                    return $"Recent actions:\n{entries}";

                case Intent.TellMeMore:
                {
                    if (_stateMachine.State == ConversationState.BrowsingCategory && _stateMachine.CurrentTopic != null)
                        return _catalog.FormatTopicMenu(_stateMachine.CurrentTopic);
                    var follow = _chat.TryFollowUp("more");
                    return follow ?? "Sure - which topic? Try password, phishing, scams, privacy, 2FA, malware, Wi-Fi, or safe browsing.";
                }

                case Intent.TopicQuestion:
                    return EnterTopic(result.Topic);

                case Intent.Sentiment:
                    return _chat.TrySentiment(result.Payload ?? string.Empty) ?? FallbackHelp();

                case Intent.SmallTalk:
                    return _chat.TrySmallTalk(result.Payload ?? string.Empty) ?? FallbackHelp();

                case Intent.Goodbye:
                    _stateMachine.Reset();
                    return "Goodbye! Stay safe online.";

                default:
                    var legacyTopic = FindLegacyTopic((result.Payload ?? string.Empty).ToLowerInvariant());
                    if (legacyTopic != null) return EnterTopic(legacyTopic.Slug);
                    return FallbackHelp();
            }
        }

        private string HandleMenuSelection(int n)
        {
            if (_stateMachine.State == ConversationState.BrowsingCategory && _stateMachine.CurrentTopic != null)
            {
                var topic = _stateMachine.CurrentTopic;
                var question = topic.Questions.FirstOrDefault(q => q.Number == n);
                if (question == null)
                    return $"Sorry, I don't have option {n} for {topic.DisplayName}. Try a number from 1 to {topic.Questions.Count}, or /categories to switch topics.";
                _log.Log("Chat", $"Answered Q{n} in topic '{topic.Slug}'.");
                return _catalog.FormatAnswerWithSuggestion(topic, question);
            }

            var picked = _catalog.FindByOrdinal(n);
            if (picked != null) return EnterTopic(picked.Slug);

            return "I'm not sure what option you meant. Type /categories to see the topic list, or just ask me a question.";
        }

        private string EnterTopic(string? slug)
        {
            var topic = _catalog.FindBySlug(slug ?? string.Empty);
            if (topic == null) return FallbackHelp();
            _stateMachine.EnterTopic(topic);
            _profile.FavouriteTopic = topic.Slug;
            _log.Log("Chat", $"Entered topic '{topic.Slug}'.");
            return _catalog.FormatTopicMenu(topic);
        }

        private Topic? FindLegacyTopic(string lower)
        {
            string[] aliases = { "scam", "wifi", "safe browsing", "update" };
            foreach (var legacy in aliases)
            {
                if (lower.Contains(legacy))
                {
                    var fallback = _catalog.FindBySlug("phishing");
                    return fallback;
                }
            }
            return _catalog.FindByInput(lower);
        }

        private string FallbackHelp()
        {
            return "I didn't quite understand that. You can:\n" +
                "  - Type /help to see all commands\n" +
                "  - Type /categories to browse cybersecurity topics\n" +
                "  - Ask me a question like 'how do I spot phishing?'\n" +
                "  - Add a task: 'add task to enable 2FA'";
        }

        private static DateTime? ParseDate(string lower)
        {
            if (lower.Contains("tomorrow"))
                return DateTime.Today.AddDays(1);
            if (lower.Contains("today"))
                return DateTime.Today;
            if (lower.Contains("next week"))
                return DateTime.Today.AddDays(7);

            var m = InDaysRegex.Match(lower);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var days))
                return DateTime.Today.AddDays(days);

            return null;
        }

        private static string CleanPayload(string payload)
        {
            payload = payload.Trim().TrimEnd('.', '!', '?');
            payload = InDaysRegex.Replace(payload, string.Empty).Trim();
            payload = Regex.Replace(payload, @"\b(?:tomorrow|today|next\s+week)\b", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (payload.EndsWith(" on", StringComparison.OrdinalIgnoreCase)) payload = payload.Substring(0, payload.Length - 3);
            if (payload.EndsWith(" in", StringComparison.OrdinalIgnoreCase)) payload = payload.Substring(0, payload.Length - 3);
            if (payload.Length == 0) return "Untitled task";
            return char.ToUpper(payload[0]) + payload.Substring(1);
        }

        private static bool ContainsSentiment(string lower)
        {
            string[] words = { "worried", "scared", "frustrated", "curious", "confused", "angry" };
            return words.Any(w => lower.Contains(w));
        }

        private static bool IsSmallTalk(string lower)
        {
            string[] phrases = { "how are you", "purpose", "what can i ask", "thanks", "thank you" };
            return phrases.Any(p => lower.Contains(p));
        }
    }
}
