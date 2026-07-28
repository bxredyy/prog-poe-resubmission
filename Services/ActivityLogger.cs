// ActivityLogger.cs
// Keeps a list of everything the bot does (task added, quiz taken, reminder set, etc.).
// Shows the most recent 10 by default, with a "Show All" button for the full list.
// POE Part 3 Task 4: Activity Log Feature.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class ActivityLogger
    {
        private readonly List<ActivityLogEntry> _all = new();
        public ObservableCollection<ActivityLogEntry> RecentView { get; } = new();
        public int DisplayLimit { get; }

        public event EventHandler<ActivityLogEntry>? EntryAdded;

        public ActivityLogger(int displayLimit)
        {
            DisplayLimit = displayLimit < 5 ? 10 : displayLimit;
        }

        public IReadOnlyList<ActivityLogEntry> All => _all;

        public void Log(string category, string description)
        {
            var entry = new ActivityLogEntry
            {
                Id = _all.Count + 1,
                Timestamp = DateTime.Now,
                Category = category,
                Description = description
            };
            _all.Add(entry);
            RecentView.Insert(0, entry);
            while (RecentView.Count > DisplayLimit)
            {
                RecentView.RemoveAt(RecentView.Count - 1);
            }
            EntryAdded?.Invoke(this, entry);
        }

        public IReadOnlyList<ActivityLogEntry> ShowMore() => _all;
    }
}
