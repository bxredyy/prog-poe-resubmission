// MainWindow.xaml.cs
// The main window. Shows the ASCII banner up top, four tabs in the middle
// (Chat, Tasks, Quiz, Activity Log), and a status bar at the bottom.
// Plays the voice greeting on first load.
// POE Part 1: ASCII art + Voice. Part 2/3: GUI shell.
//
// References:
//   GeeksforGeeks. C# Events.
//     https://www.geeksforgeeks.org/c-sharp/c-sharp-events/
//   Microsoft Docs. Window class (WPF).
//     https://learn.microsoft.com/en-us/dotnet/api/system.windows.window

using System.Windows;
using CyberSecurityBot.Models;
using CyberSecurityBot.Services;

namespace CyberSecurityBot.Views
{
    public partial class MainWindow : Window
    {
        // The ASCII art banner shown at the top of the window.
        // POE Part 1: Image Display (ASCII art).
        private const string AsciiBanner =
            "   _____      _               ____        _   \n" +
            "  / ____|    | |             |  _ \\      | |  \n" +
            " | |    _   _| |__   ___ _ __| |_) | ___ | |_ \n" +
            " | |   | | | | '_ \\ / _ \\ '__|  _ < / _ \\| __|\n" +
            " | |___| |_| | |_) |  __/ |  | |_) | (_) | |_ \n" +
            "  \\_____\\__, |_.__/ \\___|_|  |____/ \\___/ \\__|\n" +
            "         __/ |                                \n" +
            "        |___/                                 \n";

        private ServiceContainer _services;

        public MainWindow()
        {
            InitializeComponent();
            _services = App.Services;

            BannerText.Text = AsciiBanner;
            WireUp();

            // Use the Loaded event so the audio plays once the window is on screen.
            this.Loaded += MainWindow_Loaded;
        }

        // Hand the service container to each tab and subscribe to the activity log.
        private void WireUp()
        {
            ChatPage.Bind(_services);
            TaskPage.Bind(_services);
            QuizPage.Bind(_services);
            LogPage.Bind(_services);

            _services.ActivityLog.EntryAdded += OnActivityLogged;
        }

        // When something new is logged, show it in the bottom status bar.
        private void OnActivityLogged(object sender, ActivityLogEntry entry)
        {
            StatusText.Text = entry.Timestamp.ToString("HH:mm:ss") + "  " + entry.Description;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // POE Part 1: Voice greeting plays when the app starts.
            _services.Voice.PlayGreeting();
            _services.ActivityLog.Log("App", "Application launched.");
        }

        private void ReplayVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            _services.Voice.PlayGreeting();
        }
    }
}
