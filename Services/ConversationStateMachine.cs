// ConversationStateMachine.cs
// Remembers what the user is doing right now - chatting, browsing a category, or taking a quiz.
// So when the user types "2", the bot knows if that means menu option 2 or sub question 2.
// Also remembers the last quiz score for /score.
// POE Part 2: Conversation Flow + Memory and Recall.

using System;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class ConversationStateMachine
    {
        public ConversationState State { get; private set; } = ConversationState.Idle;
        public Topic? CurrentTopic { get; private set; }
        public int LastQuizScore { get; private set; }
        public int LastQuizTotal { get; private set; }
        public int QuizAttempts { get; private set; }

        public event EventHandler? StateChanged;

        public void EnterTopic(Topic topic)
        {
            State = ConversationState.BrowsingCategory;
            CurrentTopic = topic;
            Raise();
        }

        public void ExitTopic()
        {
            State = ConversationState.Idle;
            CurrentTopic = null;
            Raise();
        }

        public void EnterChatQuiz()
        {
            State = ConversationState.InChatQuiz;
            Raise();
        }

        public void ExitChatQuiz()
        {
            State = ConversationState.Idle;
            Raise();
        }

        public void RecordQuizResult(int score, int total)
        {
            LastQuizScore = score;
            LastQuizTotal = total;
            QuizAttempts++;
        }

        public void Reset()
        {
            State = ConversationState.Idle;
            CurrentTopic = null;
            Raise();
        }

        private void Raise() => StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
