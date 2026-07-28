// TaskView.xaml.cs
// The Tasks tab. Lets the user add a new task (with optional reminder date),
// mark tasks as done, or delete them. The list is bound to TaskService.Tasks
// so it updates automatically when anything changes.
// POE Part 3 Task 1: Task Assistant with Reminders.

using System.Windows;
using System.Windows.Controls;
using CyberSecurityBot.Models;
using CyberSecurityBot.Services;

namespace CyberSecurityBot.Views
{
    public partial class TaskView : UserControl
    {
        private ServiceContainer? _services;

        public TaskView()
        {
            InitializeComponent();
        }

        public void Bind(ServiceContainer services)
        {
            _services = services;
            TaskList.ItemsSource = services.Tasks.Tasks;
            DbStatusText.Text = services.Database.Status;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null) return;
            var title = (TitleBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.Text = "Please enter a title.";
                return;
            }
            var task = _services.Tasks.Add(title, (DescriptionBox.Text ?? string.Empty).Trim(), ReminderPicker.SelectedDate);
            StatusLabel.Text = $"Added '{task.Title}'.";
            TitleBox.Clear();
            DescriptionBox.Clear();
            ReminderPicker.SelectedDate = null;
        }

        private void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null) return;
            if (sender is Button btn && btn.Tag is CyberTask task)
            {
                _services.Tasks.MarkDone(task);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null) return;
            if (sender is Button btn && btn.Tag is CyberTask task)
            {
                _services.Tasks.Delete(task);
            }
        }
    }
}
