// ChatService.cs
// The "brain" for chatting holds the keyword bank, random replies,
// the user's name/favourite topic (memory), and the sentiment dictionary.
// POE Part 2: Keyword Recognition, Random Responses, Memory and Recall, Sentiment.

using System;
using System.Collections.Generic;
using System.Linq;
using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class ChatService
    {
        private readonly UserProfile _profile;
        private readonly ActivityLogger _log;
        private readonly Random _random = new();

        private readonly Dictionary<string, List<string>> _topicResponses;
        private readonly Dictionary<string, List<string>> _smallTalk;
        private readonly Dictionary<string, string> _sentimentReplies;

        public string? LastTopic { get; private set; }

        public ChatService(UserProfile profile, ActivityLogger log)
        {
            _profile = profile;
            _log = log;

            _topicResponses = new(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new()
                {
                    "Use long, unique passwords for every account. A 14+ character passphrase beats a short complex one.",
                    "A password manager (Bitwarden, 1Password) means you only need to remember one strong master password.",
                    "Never reuse passwords across sites. If one site is breached, every shared password becomes a key."
                },
                ["phishing"] = new()
                {
                    "Phishing emails often create false urgency. Slow down and verify the sender before clicking anything.",
                    "Hover over links to see the real URL. A 'bank' email pointing to a random domain is a red flag.",
                    "Banks and SARS will never ask for your full PIN or OTP by email or SMS. If they do, it's a scam."
                },
                ["privacy"] = new()
                {
                    "Review the privacy settings on your social media accounts every few months - defaults change.",
                    "Share less than you think. Birthdays, schools, and pet names are often used as security answers.",
                    "Use a separate email for sign-ups so your main inbox stays clean and harder to phish."
                },
                ["scam"] = new()
                {
                    "If a deal sounds too good to be true, it almost always is. Verify the seller independently.",
                    "Romance and crypto scams often start friendly and slowly escalate. Trust your instincts, not the pressure.",
                    "Never pay 'release fees' or 'taxes' to claim a prize. Legitimate prizes never require upfront payments."
                },
                ["2fa"] = new()
                {
                    "Two-factor authentication (2FA) adds a second step - even if your password leaks, your account stays safe.",
                    "Prefer authenticator apps (Google Authenticator, Authy) over SMS codes when possible; SIM-swap attacks happen.",
                    "Turn on 2FA for your email first. If an attacker controls your email, they can reset every other account."
                },
                ["malware"] = new()
                {
                    "Only install apps from official stores or vetted websites. Pirated software is a top malware source.",
                    "Keep Windows Defender or another reputable antivirus turned on - it catches most common threats.",
                    "Back up important files to an external drive or cloud. Ransomware is much less scary when you have backups."
                },
                ["update"] = new()
                {
                    "Software updates aren't just features - they patch security holes. Don't postpone them indefinitely.",
                    "Turn on automatic updates for your OS, browser, and antivirus. Most attacks exploit known, already-patched bugs.",
                    "Old routers and IoT devices are common attack surfaces. Check for firmware updates a few times a year."
                },
                ["wifi"] = new()
                {
                    "Avoid sensitive logins on public Wi-Fi. If you must, use a reputable VPN.",
                    "At home, change the default router password and use WPA2 or WPA3 encryption.",
                    "Hide-your-SSID isn't real security - a strong password is what actually matters."
                },
                ["safe browsing"] = new()
                {
                    "Look for the padlock and the correct domain, not just HTTPS - phishing sites use HTTPS too.",
                    "Use an ad blocker like uBlock Origin; many drive-by malware attacks come through ads.",
                    "Bookmark important sites (banking, work) and use the bookmarks rather than searching."
                }
            };

            _smallTalk = new(StringComparer.OrdinalIgnoreCase)
            {
                ["how are you"] = new()
                {
                    "I'm running fine, thanks for asking! Ready to chat about staying safe online.",
                    "All systems green. What cybersecurity topic should we look at?"
                },
                ["purpose"] = new()
                {
                    "My purpose is to help South African citizens stay safe online - phishing, passwords, scams, the works.",
                    "I'm a cybersecurity awareness assistant. Ask me anything about online safety."
                },
                ["what can i ask"] = new()
                {
                    "Try asking about: passwords, phishing, scams, privacy, 2FA, safe browsing, malware, updates, or Wi-Fi. You can also add tasks, take a quiz, or view the activity log.",
                    "Topics I know: passwords, phishing, scams, privacy, 2FA, safe browsing, malware, updates, Wi-Fi. I can also manage tasks, run a quiz, and show what I've done."
                },
                ["hello"] = new()
                {
                    "Hi! What would you like to learn about today?",
                    "Hello! Ready when you are - ask about any cybersecurity topic."
                },
                ["thanks"] = new()
                {
                    "Anytime! Stay safe out there.",
                    "Happy to help. Ask me anything else!"
                }
            };

            _sentimentReplies = new(StringComparer.OrdinalIgnoreCase)
            {
                ["worried"] = "It's completely understandable to feel worried - cybercrime is real, but a few habits go a long way. Let me share something useful:",
                ["scared"] = "You're not alone in feeling that way. The good news: the basics protect you against most threats. Here's a starting point:",
                ["frustrated"] = "I hear you - security can feel overwhelming. Let's break it down into one small step at a time:",
                ["curious"] = "Great curiosity! Cybersecurity is more interesting than people think. Here's a tip you'll like:",
                ["confused"] = "No worries, let's clear that up. Here's a simpler way to think about it:",
                ["angry"] = "Totally fair to be angry - scams are designed to take advantage of people. Here's how to push back:"
            };
        }

        public string Greet()
        {
            var name = string.IsNullOrWhiteSpace(_profile.Name) ? "friend" : _profile.Name;
            return $"Hi {name}! I'm your Cybersecurity Awareness Bot. Ask me about passwords, phishing, scams, privacy, 2FA, malware, Wi-Fi, or say 'help' to see everything I can do.";
        }

        public string WelcomeMessage()
        {
            var name = string.IsNullOrWhiteSpace(_profile.Name) ? string.Empty : $", {_profile.Name}";
            return
                $"Welcome to the Cybersecurity Awareness Bot{name}!\n\n" +
                "I can help you learn about:\n" +
                "  1. Passwords\n" +
                "  2. Phishing\n" +
                "  3. Privacy\n" +
                "  4. Two-Factor Authentication\n" +
                "  5. Malware\n\n" +
                "Try saying 'help me with passwords', or use one of the quick commands:\n" +
                "  /help · /categories · /quiz · /tips · /about\n" +
                "You can also tell me your name ('My name is Alex') so I can personalise tips for you.";
        }

        public string PickRandomTip()
        {
            var topicKeys = _topicResponses.Keys.ToList();
            var topic = topicKeys[_random.Next(topicKeys.Count)];
            var tips = _topicResponses[topic];
            var pick = tips[_random.Next(tips.Count)];
            _log.Log("Tip", $"Shared a random tip about '{topic}'.");
            return $"{char.ToUpper(topic[0])}{topic.Substring(1)} tip: {Personalise(pick)}";
        }

        public string RespondToTopic(string topic)
        {
            if (_topicResponses.TryGetValue(topic, out var responses))
            {
                LastTopic = topic;
                _profile.FavouriteTopic = topic;
                _log.Log("Chat", $"Responded on topic '{topic}'.");
                var pick = responses[_random.Next(responses.Count)];
                return Personalise(pick);
            }
            return string.Empty;
        }

        public string? TryFollowUp(string input)
        {
            var lower = input.ToLowerInvariant();
            if (LastTopic == null) return null;
            if (lower.Contains("another") || lower.Contains("more") || lower.Contains("explain") || lower.Contains("tell me") || lower.Contains("again"))
            {
                return RespondToTopic(LastTopic);
            }
            return null;
        }

        public string? TrySmallTalk(string input)
        {
            var lower = input.ToLowerInvariant();
            foreach (var kvp in _smallTalk)
            {
                if (lower.Contains(kvp.Key))
                {
                    var pick = kvp.Value[_random.Next(kvp.Value.Count)];
                    return Personalise(pick);
                }
            }
            return null;
        }

        public string? TrySentiment(string input)
        {
            var lower = input.ToLowerInvariant();
            foreach (var kvp in _sentimentReplies)
            {
                if (lower.Contains(kvp.Key))
                {
                    _profile.LastSentiment = kvp.Key;
                    _log.Log("Sentiment", $"Detected sentiment '{kvp.Key}'.");
                    var topic = ExtractTopic(lower) ?? LastTopic ?? "phishing";
                    var tip = RespondToTopic(topic);
                    return $"{kvp.Value} {tip}";
                }
            }
            return null;
        }

        public IEnumerable<string> KnownTopics() => _topicResponses.Keys;

        public string? ExtractTopic(string input)
        {
            foreach (var topic in _topicResponses.Keys)
            {
                if (input.Contains(topic, StringComparison.OrdinalIgnoreCase))
                    return topic;
            }
            return null;
        }

        public string Personalise(string text)
        {
            if (string.IsNullOrWhiteSpace(_profile.Name)) return text;
            if (text.Contains(_profile.Name, StringComparison.OrdinalIgnoreCase))
                return text;
            return $"{_profile.Name}, {char.ToLower(text[0])}{text.Substring(1)}";
        }

        public string Fallback() =>
            "I'm not sure I understood that. Try asking about passwords, phishing, scams, privacy, 2FA, malware, or Wi-Fi - or say 'add task', 'start quiz', or 'show activity log'.";
    }
}
