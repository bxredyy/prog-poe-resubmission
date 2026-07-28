// TaskService.cs
// Manages the user's cybersecurity tasks (add, mark done, delete, edit).
// Talks to the DatabaseService so tasks stay saved between runs.
// POE Part 3 Task 1: Task Assistant with Reminders.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class TaskService
    {
        private readonly DatabaseService _db;
        private readonly ActivityLogger _log;
        private int _nextLocalId = 1;

        public ObservableCollection<CyberTask> Tasks { get; } = new();

        public TaskService(DatabaseService db, ActivityLogger log)
        {
            _db = db;
            _log = log;
            ReloadFromDatabase();
        }

        public void ReloadFromDatabase()
        {
            Tasks.Clear();
            if (!_db.IsAvailable) return;

            try
            {
                foreach (var task in _db.GetAllTasks())
                {
                    Tasks.Add(task);
                    if (task.Id >= _nextLocalId) _nextLocalId = task.Id + 1;
                }
            }
            catch (Exception ex)
            {
                _log.Log("Database", $"Failed to load tasks: {ex.Message}");
            }
        }

        public CyberTask Add(string title, string description, DateTime? reminder)
        {
            var task = new CyberTask
            {
                Title = title,
                Description = description ?? string.Empty,
                ReminderAt = reminder,
                Status = CyberTaskStatus.Pending,
                CreatedAt = DateTime.Now
            };

            if (_db.IsAvailable)
            {
                try { task.Id = _db.InsertTask(task); }
                catch (Exception ex)
                {
                    _log.Log("Database", $"InsertTask failed: {ex.Message}");
                    task.Id = _nextLocalId++;
                }
            }
            else
            {
                task.Id = _nextLocalId++;
            }

            Tasks.Insert(0, task);

            var reminderText = reminder.HasValue ? $" (reminder set for {reminder:yyyy-MM-dd})" : string.Empty;
            _log.Log("Task", $"Task added: '{title}'{reminderText}.");
            return task;
        }

        public void MarkDone(CyberTask task)
        {
            task.Status = CyberTaskStatus.Done;
            task.CompletedAt = DateTime.Now;
            if (_db.IsAvailable)
            {
                try { _db.UpdateTaskStatus(task.Id, CyberTaskStatus.Done); }
                catch (Exception ex) { _log.Log("Database", $"UpdateTaskStatus failed: {ex.Message}"); }
            }
            _log.Log("Task", $"Marked '{task.Title}' as completed.");
            Refresh(task);
        }

        public void Delete(CyberTask task)
        {
            if (_db.IsAvailable)
            {
                try { _db.DeleteTask(task.Id); }
                catch (Exception ex) { _log.Log("Database", $"DeleteTask failed: {ex.Message}"); }
            }
            Tasks.Remove(task);
            _log.Log("Task", $"Deleted task '{task.Title}'.");
        }

        public void Update(CyberTask task)
        {
            if (_db.IsAvailable)
            {
                try { _db.UpdateTask(task); }
                catch (Exception ex) { _log.Log("Database", $"UpdateTask failed: {ex.Message}"); }
            }
            _log.Log("Task", $"Updated task '{task.Title}'.");
            Refresh(task);
        }

        private void Refresh(CyberTask task)
        {
            var idx = Tasks.IndexOf(task);
            if (idx >= 0)
            {
                Tasks.RemoveAt(idx);
                Tasks.Insert(idx, task);
            }
        }
    }
}
