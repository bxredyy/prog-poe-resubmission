// UserProfile.cs
// Stores the user's name, their favourite topic, and the last sentiment we detected.
// This is the "memory" of the bot - POE Part 2: Memory and Recall.
// Reference: GeeksforGeeks - C# Properties https://www.geeksforgeeks.org/c-sharp/c-sharp-properties/

namespace CyberSecurityBot.Models
{
    public class UserProfile
    {
        public string Name { get; set; } = "";
        public string FavouriteTopic { get; set; } = "";
        public string LastSentiment { get; set; } = "";
    }
}
