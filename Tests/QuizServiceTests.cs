using CyberSecurityBot.Services;
using Xunit;

namespace CyberSecurityBot.Tests
{
    public class QuizServiceTests
    {
        [Fact]
        public void Quiz_has_at_least_twelve_questions()
        {
            var log = new ActivityLogger(10);
            var quiz = new QuizService(log);
            quiz.Start();
            Assert.True(quiz.Total >= 12);
        }

        [Fact]
        public void Score_increments_on_correct_answer()
        {
            var log = new ActivityLogger(10);
            var quiz = new QuizService(log);
            var first = quiz.Start();
            var (correct, _, _, _) = quiz.Submit(first.CorrectIndex);
            Assert.True(correct);
            Assert.Equal(1, quiz.Score);
        }

        [Fact]
        public void Final_feedback_describes_score()
        {
            var log = new ActivityLogger(10);
            var quiz = new QuizService(log);
            quiz.Start();
            while (quiz.IsActive)
            {
                quiz.Submit(0);
            }
            var feedback = quiz.FinalFeedback();
            Assert.False(string.IsNullOrWhiteSpace(feedback));
        }
    }
}
