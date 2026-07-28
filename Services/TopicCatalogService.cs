// TopicCatalogService.cs
// Holds the 5 cybersecurity categories (Passwords, Phishing, Privacy, 2FA, Malware)
// and the sub-questions for each one. Used when the user says "tell me about phishing"
// or picks a category with /categories.
// POE Part 2: Keyword Recognition; Part 3: NLP Simulation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class TopicCatalogService
    {
        public IReadOnlyList<Topic> Topics { get; }

        public TopicCatalogService()
        {
            Topics = BuildTopics();
        }

        public Topic? FindBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            return Topics.FirstOrDefault(t =>
                string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.DisplayName, slug, StringComparison.OrdinalIgnoreCase));
        }

        public Topic? FindByInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            foreach (var t in Topics)
            {
                if (input.Contains(t.Slug, StringComparison.OrdinalIgnoreCase) ||
                    input.Contains(t.DisplayName, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
        }

        public Topic? FindByOrdinal(int oneBased)
        {
            if (oneBased < 1 || oneBased > Topics.Count) return null;
            return Topics[oneBased - 1];
        }

        public string FormatCategoryList()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Here are the cybersecurity categories I can guide you through:");
            for (int i = 0; i < Topics.Count; i++)
            {
                sb.AppendLine($"  {i + 1}. {Topics[i].DisplayName} - {Topics[i].Overview}");
            }
            sb.Append("Reply with the number, the topic name, or say 'help me with passwords'.");
            return sb.ToString();
        }

        public string FormatTopicMenu(Topic topic)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{topic.DisplayName} - {topic.Overview}");
            sb.AppendLine("What would you like to know?");
            foreach (var q in topic.Questions)
            {
                sb.AppendLine($"  {q.Number}. {q.Prompt}");
            }
            sb.Append("Reply with a number to pick a question, or ask in your own words.");
            return sb.ToString();
        }

        public string FormatAnswerWithSuggestion(Topic topic, TopicQuestion question)
        {
            var sb = new StringBuilder();
            sb.AppendLine(question.Answer);
            sb.AppendLine();
            var next = FindBySlug(topic.SuggestedNextSlug);
            if (next != null)
            {
                sb.Append($"Next, you might want to learn about {next.DisplayName}. Say '{next.DisplayName.ToLower()}' or '/categories' to keep exploring.");
            }
            else
            {
                sb.Append("Ask another question on this topic, or type /categories to switch topics.");
            }
            return sb.ToString();
        }

        private static List<Topic> BuildTopics() => new()
        {
            new Topic
            {
                Slug = "passwords",
                DisplayName = "Passwords",
                Overview = "The foundation of online safety.",
                SuggestedNextSlug = "2fa",
                Questions = new List<TopicQuestion>
                {
                    new() { Number = 1, Prompt = "What makes a strong password?",
                            Answer = "A strong password is long (14+ characters), unpredictable, and unique to each account. Use a passphrase made of unrelated words ('correct-horse-battery-staple-72'), or let a password manager generate a random string. Avoid personal info, dictionary words, and predictable substitutions like '@' for 'a'." },
                    new() { Number = 2, Prompt = "Should I reuse passwords?",
                            Answer = "Never reuse passwords. If one site is breached, every account sharing that password becomes vulnerable. A password manager lets you have a unique password per site while you only remember one strong master password." },
                    new() { Number = 3, Prompt = "How often should I change my password?",
                            Answer = "Modern guidance: change a password only when there's a reason - a breach, a suspicious login, or a shared device. Forced rotation often pushes people to weaker passwords like 'Password2!'. Focus on length, uniqueness, and 2FA instead." }
                }
            },
            new Topic
            {
                Slug = "phishing",
                DisplayName = "Phishing",
                Overview = "Fake messages designed to trick you.",
                SuggestedNextSlug = "malware",
                Questions = new List<TopicQuestion>
                {
                    new() { Number = 1, Prompt = "What is phishing?",
                            Answer = "Phishing is when an attacker pretends to be someone you trust - a bank, courier, employer - and tricks you into clicking a link, downloading a file, or sharing credentials. It usually arrives by email, SMS ('smishing'), or DM." },
                    new() { Number = 2, Prompt = "How do I identify a phishing email?",
                            Answer = "Watch for: urgent threats ('your account will be locked'), generic greetings, sender domains that don't match the brand, hovered URLs that point to random domains, and unexpected attachments. When in doubt, contact the company directly using a number you already trust." },
                    new() { Number = 3, Prompt = "What should I do after clicking a phishing link?",
                            Answer = "1) Disconnect from the internet. 2) Change your passwords - start with email. 3) Enable 2FA on every important account. 4) Run a malware scan. 5) Report the phishing email to your IT team or bank." }
                }
            },
            new Topic
            {
                Slug = "privacy",
                DisplayName = "Privacy",
                Overview = "Controlling what others know about you.",
                SuggestedNextSlug = "passwords",
                Questions = new List<TopicQuestion>
                {
                    new() { Number = 1, Prompt = "How can I protect my personal information?",
                            Answer = "Share the minimum. Use a separate email for sign-ups. Lock down social media profiles. Don't post your live location. Review app permissions monthly and revoke anything you don't actively use." },
                    new() { Number = 2, Prompt = "What are privacy settings?",
                            Answer = "Privacy settings control who can see your data and how apps share it. Defaults are rarely the most private choice - go into Google, Facebook, Instagram, and Microsoft account settings and tighten visibility, ad personalisation, and data-sharing toggles." }
                }
            },
            new Topic
            {
                Slug = "2fa",
                DisplayName = "Two-Factor Authentication",
                Overview = "A second proof of identity beyond your password.",
                SuggestedNextSlug = "passwords",
                Questions = new List<TopicQuestion>
                {
                    new() { Number = 1, Prompt = "Why use 2FA?",
                            Answer = "Even if an attacker steals your password, 2FA blocks them because they don't have your second factor (an app code, a physical key, or a biometric). Turn it on for email and banking first - those unlock everything else." },
                    new() { Number = 2, Prompt = "What happens if I lose my phone?",
                            Answer = "Save your 2FA backup codes when you first set it up - store them in your password manager or printed somewhere safe. Most authenticator apps (Authy, 1Password) also sync across devices, so you can restore them on a new phone." }
                }
            },
            new Topic
            {
                Slug = "malware",
                DisplayName = "Malware",
                Overview = "Hostile software like viruses and ransomware.",
                SuggestedNextSlug = "phishing",
                Questions = new List<TopicQuestion>
                {
                    new() { Number = 1, Prompt = "What is malware?",
                            Answer = "Malware is any software designed to harm - viruses, worms, trojans, spyware, ransomware. It can steal data, lock your files for ransom, or quietly turn your device into part of a botnet." },
                    new() { Number = 2, Prompt = "How do malware infections happen?",
                            Answer = "Common routes: pirated software, malicious email attachments, fake software updates, drive-by downloads from compromised sites, and infected USB drives. Keep your OS patched, use a reputable antivirus, and never install software from unknown sources." }
                }
            }
        };
    }
}
