// Topic.cs
// Plain data class for a cybersecurity category and its sub-questions.
// "Slug" is the short name used in code, "DisplayName" is the pretty one.
// Reference: GeeksforGeeks - C# Properties https://www.geeksforgeeks.org/c-sharp/c-sharp-properties/

using System.Collections.Generic;

namespace CyberSecurityBot.Models
{
    public class Topic
    {
        public string Slug { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Overview { get; set; } = "";
        public string SuggestedNextSlug { get; set; } = "";
        public List<TopicQuestion> Questions { get; set; } = new List<TopicQuestion>();
    }

    public class TopicQuestion
    {
        public int Number { get; set; }
        public string Prompt { get; set; } = "";
        public string Answer { get; set; } = "";
    }
}
