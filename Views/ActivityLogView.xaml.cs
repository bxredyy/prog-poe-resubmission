// ActivityLogView.xaml.cs
// The Activity Log tab. Shows the most recent 10 actions by default.
// "Show All" switches to the full history and "Show Recent" goes back.
// POE Part 3 Task 4: Activity Log Feature.

using System.Windows;
using System.Windows.Controls;
using CyberSecurityBot.Services;

namespace CyberSecurityBot.Views
{
    public partial class ActivityLogView : UserControl
    {
        private ServiceContainer? _services;

        public ActivityLogView()
        {
            InitializeComponent();
        }

        public void Bind(ServiceContainer services)
        {
            _services = services;
            ShowRecent();
            LimitText.Text = $"Showing the last {services.ActivityLog.DisplayLimit} actions. Click 'Show All' for full history.";
        }

        private void ShowRecent()
        {
            if (_services == null) return;
            EntryList.ItemsSource = _services.ActivityLog.RecentView;
            ShowMoreButton.IsEnabled = true;
            ShowRecentButton.IsEnabled = false;
        }

        private void ShowMore_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null) return;
            EntryList.ItemsSource = _services.ActivityLog.All;
            ShowMoreButton.IsEnabled = false;
            ShowRecentButton.IsEnabled = true;
        }

        private void ShowRecent_Click(object sender, RoutedEventArgs e) => ShowRecent();
    }
}
