using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class ActivityLoggerTests
    {
        [Fact]
        public void RecentView_caps_at_display_limit()
        {
            var log = new ActivityLogger(5);
            for (int i = 0; i < 12; i++)
                log.Log("Test", $"Entry {i}");
            Assert.Equal(5, log.RecentView.Count);
            Assert.Equal(12, log.All.Count);
        }

        [Fact]
        public void Most_recent_entry_appears_first()
        {
            var log = new ActivityLogger(10);
            log.Log("A", "first");
            log.Log("B", "second");
            Assert.Equal("second", log.RecentView[0].Description);
            Assert.Equal("first", log.RecentView[1].Description);
        }
    }
}
