// QuizService.cs
// Holds the 13 quiz questions, tracks the current question and the user's score,
// and gives a final feedback message based on how well they did.
// POE Part 3 Task 2: Cybersecurity Mini-Game (Quiz).
//
// References:
//   GeeksforGeeks. C# Tuples.
//     https://www.geeksforgeeks.org/c-sharp/c-sharp-tuples/
//   GeeksforGeeks. C# List<T>.
//     https://www.geeksforgeeks.org/c-sharp/c-sharp-list-class/

using System;
using System.Collections.Generic;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class QuizService
    {
        private readonly ActivityLogger _log;
        private readonly List<QuizQuestion> _bank;
        private List<QuizQuestion> _current = new List<QuizQuestion>();
        private int _index;
        private int _score;
        private Random _shuffler = new Random();

        public int Index { get { return _index; } }
        public int Score { get { return _score; } }
        public int Total { get { return _current.Count; } }
        public bool IsActive { get; private set; }

        public QuizService(ActivityLogger log)
        {
            _log = log;
            _bank = BuildBank();
        }

        // Start a fresh quiz. Shuffles the questions and returns the first one.
        public QuizQuestion Start()
        {
            _current = ShuffleQuestions(_bank);
            _index = 0;
            _score = 0;
            IsActive = true;
            _log.Log("Quiz", "Quiz started (" + _current.Count + " questions).");
            return _current[0];
        }

        // Simple shuffle using Random. Reference: GeeksforGeeks - Shuffle a List
        //   https://www.geeksforgeeks.org/c-sharp/c-program-to-shuffle-a-list/
        private List<QuizQuestion> ShuffleQuestions(List<QuizQuestion> source)
        {
            List<QuizQuestion> copy = new List<QuizQuestion>(source);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = _shuffler.Next(i + 1);
                QuizQuestion temp = copy[i];
                copy[i] = copy[j];
                copy[j] = temp;
            }
            return copy;
        }

        // Submit the user's answer for the current question.
        // Returns whether they got it right, the explanation, the next question (or null), and a "finished" flag.
        public (bool correct, string explanation, QuizQuestion next, bool finished) Submit(int selectedIndex)
        {
            if (!IsActive || _index >= _current.Count)
                return (false, "Quiz is not active.", null, true);

            QuizQuestion q = _current[_index];
            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) _score++;

            _log.Log("Quiz", "Q" + (_index + 1) + ": " + (correct ? "correct" : "wrong") + ".");

            _index++;
            if (_index >= _current.Count)
            {
                IsActive = false;
                _log.Log("Quiz", "Quiz finished with score " + _score + "/" + _current.Count + ".");
                return (correct, q.Explanation, null, true);
            }
            return (correct, q.Explanation, _current[_index], false);
        }

        // Final feedback message based on the user's percentage score.
        public string FinalFeedback()
        {
            int pct;
            if (Total == 0)
                pct = 0;
            else
                pct = (int)(100.0 * _score / Total);

            if (pct >= 80)
                return "Great job! " + _score + "/" + Total + " - you're a cybersecurity pro!";
            else if (pct >= 50)
                return "Not bad - " + _score + "/" + Total + ". A few more reps and you'll be solid.";
            else
                return "You scored " + _score + "/" + Total + ". Keep learning to stay safe online - try the chat for tips!";
        }

        // The bank of quiz questions. Hardcoded for now - in a bigger project we'd load from a file.
        private static List<QuizQuestion> BuildBank()
        {
            List<QuizQuestion> bank = new List<QuizQuestion>();

            bank.Add(new QuizQuestion
            {
                Prompt = "What should you do if you receive an email asking for your password?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Forward it to friends" },
                CorrectIndex = 2,
                Explanation = "Reporting phishing emails helps prevent scams from spreading. Legitimate services never ask for your password by email."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "True or False: Using the same password across multiple sites is safe if the password is strong.",
                Kind = QuestionKind.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "If one site is breached, every site sharing that password becomes vulnerable."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "Which of these is the strongest password?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Password123!", "MyDog2010", "correct-horse-battery-staple-72", "1234abcd" },
                CorrectIndex = 2,
                Explanation = "Length and unpredictability beat short complex passwords. A long passphrase is hard to crack and easy to remember."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "What does 2FA stand for?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Two-Factor Authentication", "Two-File Archive", "Twin Firewall Architecture", "Two-Factor Authorization" },
                CorrectIndex = 0,
                Explanation = "Two-Factor Authentication adds a second proof of identity beyond your password."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "True or False: Public Wi-Fi is always safe for online banking.",
                Kind = QuestionKind.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "Open Wi-Fi can be sniffed by attackers. Use a VPN or mobile data for sensitive logins."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "Which of these is a sign of a phishing email?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Urgent threats", "Strange sender address", "Suspicious links", "All of the above" },
                CorrectIndex = 3,
                Explanation = "Urgency, mismatched sender domains, and dodgy links are the classic phishing combo."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "What is social engineering?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Building social networks", "Manipulating people to give up secrets", "A type of firewall", "Software engineering for social apps" },
                CorrectIndex = 1,
                Explanation = "Social engineering tricks humans rather than exploiting code. Awareness is the best defence."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "True or False: A padlock icon in your browser guarantees the website is safe.",
                Kind = QuestionKind.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "The padlock only means the connection is encrypted. Phishing sites also use HTTPS - always check the domain too."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "Which is the safest way to store passwords?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "A sticky note on your monitor", "A text file on your desktop", "A reputable password manager", "In your browser's saved logins only" },
                CorrectIndex = 2,
                Explanation = "Password managers encrypt your vault and sync it securely across devices."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "What should you do before clicking a link in an unexpected message?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Click quickly before it expires", "Hover to inspect the real URL", "Forward to your contacts", "Reply asking who sent it" },
                CorrectIndex = 1,
                Explanation = "Hovering shows the real destination. If the URL doesn't match what's claimed, don't click."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "True or False: Software updates are mainly cosmetic and can be ignored.",
                Kind = QuestionKind.TrueFalse,
                Options = new List<string> { "True", "False" },
                CorrectIndex = 1,
                Explanation = "Updates usually patch security vulnerabilities. Delaying them exposes you to known attacks."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "What is ransomware?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Free software with ads", "Malware that locks your files for payment", "An antivirus tool", "A type of firewall" },
                CorrectIndex = 1,
                Explanation = "Ransomware encrypts your files and demands payment to unlock them. Regular backups defeat it."
            });

            bank.Add(new QuizQuestion
            {
                Prompt = "Which of these is NOT a good privacy habit?",
                Kind = QuestionKind.MultipleChoice,
                Options = new List<string> { "Reviewing app permissions", "Using unique emails for sign-ups", "Sharing your live location publicly", "Turning on screen lock" },
                CorrectIndex = 2,
                Explanation = "Publicly sharing live location is a privacy and physical-safety risk."
            });

            return bank;
        }
    }
}
