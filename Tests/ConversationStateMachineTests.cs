using CyberSecurityBot.Models;
using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class ConversationStateMachineTests
    {
        [Fact]
        public void Initial_state_is_idle()
        {
            var sm = new ConversationStateMachine();
            Assert.Equal(ConversationState.Idle, sm.State);
            Assert.Null(sm.CurrentTopic);
        }

        [Fact]
        public void EnterTopic_transitions_to_browsing()
        {
            var sm = new ConversationStateMachine();
            var catalog = new TopicCatalogService();
            var topic = catalog.FindBySlug("phishing")!;
            sm.EnterTopic(topic);
            Assert.Equal(ConversationState.BrowsingCategory, sm.State);
            Assert.Equal(topic, sm.CurrentTopic);
        }

        [Fact]
        public void Reset_returns_to_idle_and_clears_topic()
        {
            var sm = new ConversationStateMachine();
            var catalog = new TopicCatalogService();
            sm.EnterTopic(catalog.Topics[0]);
            sm.Reset();
            Assert.Equal(ConversationState.Idle, sm.State);
            Assert.Null(sm.CurrentTopic);
        }

        [Fact]
        public void RecordQuizResult_increments_attempts()
        {
            var sm = new ConversationStateMachine();
            sm.RecordQuizResult(10, 13);
            sm.RecordQuizResult(12, 13);
            Assert.Equal(2, sm.QuizAttempts);
            Assert.Equal(12, sm.LastQuizScore);
            Assert.Equal(13, sm.LastQuizTotal);
        }

        [Fact]
        public void StateChanged_fires_on_transitions()
        {
            var sm = new ConversationStateMachine();
            var fired = 0;
            sm.StateChanged += (_, _) => fired++;
            sm.EnterTopic(new TopicCatalogService().Topics[0]);
            sm.Reset();
            Assert.Equal(2, fired);
        }
    }
}
