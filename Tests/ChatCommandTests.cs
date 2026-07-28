using CyberSecurityBot.Models;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class ChatCommandTests
    {
        [Theory]
        [InlineData("/help", ChatCommand.Help)]
        [InlineData("/H", ChatCommand.Help)]
        [InlineData("/?", ChatCommand.Help)]
        [InlineData("/categories", ChatCommand.Categories)]
        [InlineData("/topics", ChatCommand.Categories)]
        [InlineData("/start", ChatCommand.Start)]
        [InlineData("/restart", ChatCommand.Start)]
        [InlineData("/quiz", ChatCommand.Quiz)]
        [InlineData("/score", ChatCommand.Score)]
        [InlineData("/stats", ChatCommand.Score)]
        [InlineData("/tips", ChatCommand.Tips)]
        [InlineData("/about", ChatCommand.About)]
        public void Parses_known_aliases(string input, ChatCommand expected)
        {
            Assert.Equal(expected, ChatCommandParser.Parse(input));
        }

        [Theory]
        [InlineData("/notacommand", ChatCommand.None)]
        [InlineData("hello", ChatCommand.None)]
        [InlineData("", ChatCommand.None)]
        [InlineData(null, ChatCommand.None)]
        public void Unknown_input_returns_none(string? input, ChatCommand expected)
        {
            Assert.Equal(expected, ChatCommandParser.Parse(input ?? string.Empty));
        }

        [Fact]
        public void LooksLikeCommand_detects_slash_prefix()
        {
            Assert.True(ChatCommandParser.LooksLikeCommand("/help"));
            Assert.True(ChatCommandParser.LooksLikeCommand("  /quiz"));
            Assert.False(ChatCommandParser.LooksLikeCommand("hello"));
            Assert.False(ChatCommandParser.LooksLikeCommand(""));
        }
    }
}
