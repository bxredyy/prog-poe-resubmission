// QuizView.xaml.cs
// The Quiz tab. Shows one question at a time with radio buttons for the options,
// gives instant feedback (green for correct, red for wrong) plus an explanation,
// and saves the final score to MySQL when the quiz finishes.
// POE Part 3 Task 2: Cybersecurity Mini-Game.

using System.Windows;
using System.Windows.Controls;
using CyberSecurityBot.Models;
using CyberSecurityBot.Services;

namespace CyberSecurityBot.Views
{
    public partial class QuizView : UserControl
    {
        private ServiceContainer? _services;
        private QuizQuestion? _current;
        private int? _selectedIndex;

        public QuizView()
        {
            InitializeComponent();
        }

        public void Bind(ServiceContainer services)
        {
            _services = services;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null) return;
            _current = _services.Quiz.Start();
            RenderQuestion();
            FeedbackText.Text = string.Empty;
            ScoreText.Text = $"Score: 0/{_services.Quiz.Total}";
            StartButton.Content = "Restart Quiz";
        }

        private void RenderQuestion()
        {
            if (_services == null || _current == null) return;
            QuestionText.Text = $"Q{_services.Quiz.Index + 1}. {_current.Prompt}";
            ProgressText.Text = $"Question {_services.Quiz.Index + 1} of {_services.Quiz.Total}";

            OptionsPanel.Children.Clear();
            _selectedIndex = null;
            NextButton.IsEnabled = false;

            for (int i = 0; i < _current.Options.Count; i++)
            {
                var radio = new RadioButton
                {
                    Content = _current.Options[i],
                    GroupName = "QuizOptions",
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = (System.Windows.Media.Brush)FindResource("TextBrush"),
                    Tag = i
                };
                radio.Checked += Option_Checked;
                OptionsPanel.Children.Add(radio);
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is int idx)
            {
                _selectedIndex = idx;
                SubmitAnswer();
            }
        }

        private void SubmitAnswer()
        {
            if (_services == null || _selectedIndex == null) return;
            var result = _services.Quiz.Submit(_selectedIndex.Value);
            FeedbackText.Foreground = (System.Windows.Media.Brush)FindResource(result.correct ? "SuccessBrush" : "DangerBrush");
            FeedbackText.Text = (result.correct ? "Correct! " : "Not quite. ") + result.explanation;
            ScoreText.Text = $"Score: {_services.Quiz.Score}/{_services.Quiz.Total}";

            foreach (var child in OptionsPanel.Children)
            {
                if (child is RadioButton rb) rb.IsEnabled = false;
            }

            if (result.finished)
            {
                NextButton.IsEnabled = false;
                FeedbackText.Text += "\n\n" + _services.Quiz.FinalFeedback();
                _services.Database.InsertQuizAttempt(_services.Quiz.Score, _services.Quiz.Total);
                _services.State.RecordQuizResult(_services.Quiz.Score, _services.Quiz.Total);
                _services.State.ExitChatQuiz();
            }
            else
            {
                _current = result.next;
                NextButton.IsEnabled = true;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            RenderQuestion();
            FeedbackText.Text = string.Empty;
        }
    }
}
