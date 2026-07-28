using CyberSecurityBot.Models;
using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class ChatServiceTests
    {
        [Fact]
        public void Recognises_at_least_nine_topics()
        {
            var profile = new UserProfile();
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            var topics = System.Linq.Enumerable.ToList(chat.KnownTopics());
            Assert.True(topics.Count >= 9, $"Expected at least 9 topics, found {topics.Count}");
        }

        [Fact]
        public void Returns_topic_response_when_keyword_present()
        {
            var profile = new UserProfile();
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            var topic = chat.ExtractTopic("how do I avoid phishing emails?");
            Assert.Equal("phishing", topic);
            var response = chat.RespondToTopic(topic!);
            Assert.False(string.IsNullOrWhiteSpace(response));
        }

        [Fact]
        public void Personalises_response_with_user_name()
        {
            var profile = new UserProfile { Name = "Mukona" };
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            var personal = chat.Personalise("Always use 2FA.");
            Assert.Contains("Mukona", personal);
        }

        [Fact]
        public void Detects_sentiment_and_responds()
        {
            var profile = new UserProfile();
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            var reply = chat.TrySentiment("I'm really worried about phishing");
            Assert.NotNull(reply);
            Assert.Equal("worried", profile.LastSentiment);
        }

        [Fact]
        public void Follow_up_returns_more_on_current_topic()
        {
            var profile = new UserProfile();
            var log = new ActivityLogger(10);
            var chat = new ChatService(profile, log);
            chat.RespondToTopic("password");
            var follow = chat.TryFollowUp("tell me more");
            Assert.NotNull(follow);
        }
    }
}
