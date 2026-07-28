using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class TopicCatalogTests
    {
        [Fact]
        public void Catalog_has_at_least_five_topics()
        {
            var catalog = new TopicCatalogService();
            Assert.True(catalog.Topics.Count >= 5, $"Expected at least 5 topics, found {catalog.Topics.Count}");
        }

        [Fact]
        public void Each_topic_has_questions()
        {
            var catalog = new TopicCatalogService();
            foreach (var topic in catalog.Topics)
            {
                Assert.NotEmpty(topic.Questions);
            }
        }

        [Fact]
        public void FindBySlug_finds_passwords()
        {
            var catalog = new TopicCatalogService();
            var topic = catalog.FindBySlug("passwords");
            Assert.NotNull(topic);
            Assert.Equal("Passwords", topic!.DisplayName);
        }

        [Fact]
        public void FindByInput_matches_natural_phrase()
        {
            var catalog = new TopicCatalogService();
            var topic = catalog.FindByInput("how do i avoid phishing emails");
            Assert.NotNull(topic);
            Assert.Equal("phishing", topic!.Slug);
        }

        [Fact]
        public void FindByOrdinal_returns_topic_by_number()
        {
            var catalog = new TopicCatalogService();
            var first = catalog.FindByOrdinal(1);
            Assert.NotNull(first);
            Assert.Equal(catalog.Topics[0].Slug, first!.Slug);
        }

        [Fact]
        public void FormatCategoryList_lists_every_topic()
        {
            var catalog = new TopicCatalogService();
            var listing = catalog.FormatCategoryList();
            foreach (var topic in catalog.Topics)
                Assert.Contains(topic.DisplayName, listing);
        }
    }
}
