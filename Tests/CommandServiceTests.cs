using CyberSecurityBot.Models;
using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class CommandServiceTests
    {
        private static CommandService BuildService(out ConversationStateMachine state)
        {
            var profile = new UserProfile();
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            var catalog = new TopicCatalogService();
            state = new ConversationStateMachine();
            return new CommandService(catalog, state, chat, log, profile);
        }

        [Fact]
        public void Help_lists_every_known_command()
        {
            var svc = BuildService(out _);
            var help = svc.Execute(ChatCommand.Help, out _, out _);
            Assert.Contains("/help", help);
            Assert.Contains("/categories", help);
            Assert.Contains("/quiz", help);
            Assert.Contains("/score", help);
            Assert.Contains("/tips", help);
            Assert.Contains("/about", help);
            Assert.Contains("/start", help);
        }

        [Fact]
        public void Categories_lists_every_topic()
        {
            var svc = BuildService(out _);
            var output = svc.Execute(ChatCommand.Categories, out _, out _);
            Assert.Contains("Passwords", output);
            Assert.Contains("Phishing", output);
            Assert.Contains("Two-Factor Authentication", output);
        }

        [Fact]
        public void Quiz_command_sets_start_flag_and_enters_chat_quiz()
        {
            var svc = BuildService(out var state);
            var output = svc.Execute(ChatCommand.Quiz, out var start, out _);
            Assert.True(start);
            Assert.Equal(ConversationState.InChatQuiz, state.State);
            Assert.Contains("quiz", output, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Score_when_no_attempts_prompts_to_start()
        {
            var svc = BuildService(out _);
            var output = svc.Execute(ChatCommand.Score, out _, out _);
            Assert.Contains("/quiz", output);
        }

        [Fact]
        public void Score_after_recording_shows_percentage()
        {
            var svc = BuildService(out var state);
            state.RecordQuizResult(10, 13);
            var output = svc.Execute(ChatCommand.Score, out _, out _);
            Assert.Contains("10/13", output);
        }

        [Fact]
        public void Start_resets_state_and_returns_welcome()
        {
            var svc = BuildService(out var state);
            var catalog = new TopicCatalogService();
            state.EnterTopic(catalog.Topics[0]);
            var output = svc.Execute(ChatCommand.Start, out _, out _);
            Assert.Equal(ConversationState.Idle, state.State);
            Assert.Contains("Welcome", output, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Tips_returns_personalised_tip()
        {
            var svc = BuildService(out _);
            var output = svc.Execute(ChatCommand.Tips, out _, out _);
            Assert.False(string.IsNullOrWhiteSpace(output));
        }
    }
}
