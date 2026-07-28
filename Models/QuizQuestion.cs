// QuizQuestion.cs
// Plain data class for one quiz question.
// Kind says whether it is multiple choice or true/false.
// Reference: GeeksforGeeks - C# Enums https://www.geeksforgeeks.org/c-sharp/c-sharp-enumeration-or-enum/

using System.Collections.Generic;

namespace CyberSecurityBot.Models
{
    public enum QuestionKind { MultipleChoice, TrueFalse }

    public class QuizQuestion
    {
        public string Prompt { get; set; } = "";
        public QuestionKind Kind { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
    }
}
