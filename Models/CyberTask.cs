// CyberTask.cs
// Plain data class for one cybersecurity task.
// Has a title, description, status (Pending/Done), and an optional reminder date.
// Reference: GeeksforGeeks - C# Properties https://www.geeksforgeeks.org/c-sharp/c-sharp-properties/

using System;

namespace CyberSecurityBot.Models
{
    public enum CyberTaskStatus { Pending, Done }

    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public CyberTaskStatus Status { get; set; } = CyberTaskStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public DateTime? ReminderAt { get; set; }

        // Used by the Tasks list in the GUI.
        public string StatusLabel
        {
            get
            {
                if (Status == CyberTaskStatus.Done) return "Completed";
                return "Pending";
            }
        }

        public string ReminderLabel
        {
            get
            {
                if (ReminderAt.HasValue) return ReminderAt.Value.ToString("yyyy-MM-dd HH:mm");
                return "No reminder";
            }
        }
    }
}
