// ActivityLogEntry.cs
// One line in the activity log - a timestamp, a category (e.g. "Task", "Quiz"),
// and a short description of what happened.

using System;

namespace CyberSecurityBot.Models
{
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";

        // Shown in the Activity Log list. Pretty-prints the entry as one line.
        public string Display
        {
            get
            {
                return "[" + Timestamp.ToString("HH:mm:ss") + "] " + Category + ": " + Description;
            }
        }
    }
}
