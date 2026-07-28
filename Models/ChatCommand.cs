// ChatCommand.cs
// Enum of slash commands the chat supports + a small parser that maps
// strings like "/help" or "/q" to the right enum value.

using System;
using System.Collections.Generic;

namespace CyberSecurityBot.Models
{
    public enum ChatCommand
    {
        None,
        Help,
        Categories,
        Start,
        Quiz,
        Score,
        Tips,
        About
    }

    public static class ChatCommandParser
    {
        private static readonly Dictionary<string, ChatCommand> Aliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["/help"]       = ChatCommand.Help,
                ["/h"]          = ChatCommand.Help,
                ["/?"]          = ChatCommand.Help,
                ["/commands"]   = ChatCommand.Help,
                ["/categories"] = ChatCommand.Categories,
                ["/topics"]     = ChatCommand.Categories,
                ["/cat"]        = ChatCommand.Categories,
                ["/start"]      = ChatCommand.Start,
                ["/restart"]    = ChatCommand.Start,
                ["/reset"]      = ChatCommand.Start,
                ["/quiz"]       = ChatCommand.Quiz,
                ["/q"]          = ChatCommand.Quiz,
                ["/score"]      = ChatCommand.Score,
                ["/stats"]      = ChatCommand.Score,
                ["/tips"]       = ChatCommand.Tips,
                ["/tip"]        = ChatCommand.Tips,
                ["/about"]      = ChatCommand.About,
                ["/info"]       = ChatCommand.About
            };

        public static bool LooksLikeCommand(string input) =>
            !string.IsNullOrWhiteSpace(input) && input.TrimStart().StartsWith('/');

        public static ChatCommand Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return ChatCommand.None;
            var first = input.Trim().Split(' ')[0];
            if (!first.StartsWith('/')) return ChatCommand.None;
            return Aliases.TryGetValue(first, out var cmd) ? cmd : ChatCommand.None;
        }
    }
}
