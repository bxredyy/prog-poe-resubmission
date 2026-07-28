using CyberSecurityBot.Models;
using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class NlpServiceTests
    {
        private static NlpService BuildNlp(out UserProfile profile, out TaskService tasks, out QuizService quiz, out ChatService chat, out ActivityLogger log, out ConversationStateMachine state, out TopicCatalogService catalog)
        {
            profile = new UserProfile();
            log = new ActivityLogger(10);
            var db = new DatabaseService();
            tasks = new TaskService(db, log);
            quiz = new QuizService(log);
            chat = new ChatService(profile, log);
            catalog = new TopicCatalogService();
            state = new ConversationStateMachine();
            var commands = new CommandService(catalog, state, chat, log, profile);
            return new NlpService(profile, tasks, quiz, chat, log, catalog, state, commands);
        }

        [Fact]
        public void Detects_add_task_intent()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            var result = nlp.Parse("add a task to enable two-factor authentication");
            Assert.Equal(Intent.AddTask, result.Intent);
            Assert.Contains("two-factor authentication", result.Payload, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Detects_reminder_with_relative_date()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            var result = nlp.Parse("remind me to update my password in 3 days");
            Assert.Equal(Intent.SetReminder, result.Intent);
            Assert.NotNull(result.ReminderDate);
            Assert.True(result.ReminderDate > System.DateTime.Today);
        }

        [Fact]
        public void Detects_quiz_start_intent()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            Assert.Equal(Intent.StartQuiz, nlp.Parse("start the quiz").Intent);
            Assert.Equal(Intent.StartQuiz, nlp.Parse("test me on cybersecurity").Intent);
        }

        [Fact]
        public void Detects_show_log_intent()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            Assert.Equal(Intent.ShowLog, nlp.Parse("show activity log").Intent);
            Assert.Equal(Intent.ShowLog, nlp.Parse("what have you done for me?").Intent);
        }

        [Fact]
        public void Detects_topic_keyword()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            Assert.Equal(Intent.TopicQuestion, nlp.Parse("tell me about phishing").Intent);
        }

        [Fact]
        public void Extracts_name_from_introduction()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            var result = nlp.Parse("hi, my name is Mukona");
            Assert.Equal(Intent.ProvideName, result.Intent);
            Assert.Equal("Mukona", result.Payload);
        }

        [Fact]
        public void Detects_slash_command()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            var result = nlp.Parse("/help");
            Assert.Equal(Intent.Command, result.Intent);
            Assert.Equal(ChatCommand.Help, result.Command);
        }

        [Fact]
        public void Detects_menu_selection_number()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out _, out _);
            var result = nlp.Parse("2");
            Assert.Equal(Intent.SelectMenuItem, result.Intent);
            Assert.Equal(2, result.MenuIndex);
        }

        [Fact]
        public void Topic_question_enters_browsing_state()
        {
            var nlp = BuildNlp(out _, out _, out _, out _, out _, out var stateMachine, out _);
            var result = nlp.Parse("tell me about passwords");
            nlp.Handle(result, out _, out _, out _);
            Assert.Equal(ConversationState.BrowsingCategory, stateMachine.State);
            Assert.NotNull(stateMachine.CurrentTopic);
            Assert.Equal("passwords", stateMachine.CurrentTopic!.Slug);
        }
    }
}
