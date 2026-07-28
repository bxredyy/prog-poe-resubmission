# Cybersecurity Awareness Bot

> PROG6221 Programming 2A - Portfolio of Evidence (Part 3)
> Author: **Mukona Mamaila** (ST10494639)
> Group 04 · IIE 2026

A WPF desktop chatbot that helps South African citizens learn safe online habits. The bot replies to cybersecurity questions, manages cybersecurity-related tasks in a MySQL database, runs a built-in quiz, simulates simple Natural Language Processing, and keeps an activity log of everything it does.

[![.NET CI](https://github.com/EMGPPT/prog6221-g4-2026-part1-bxredyy/actions/workflows/ci.yml/badge.svg)](https://github.com/EMGPPT/prog6221-g4-2026-part1-bxredyy/actions/workflows/ci.yml)

---

## Demo video
https://youtu.be/Wbikc4UA5bY
## Contents

1. [Features](#features)
2. [Requirements](#requirements)
3. [Setting up MySQL](#setting-up-mysql)
4. [Building and running](#building-and-running)
5. [Running the tests](#running-the-tests)
6. [Project layout](#project-layout)
7. [What to try in the chat](#what-to-try-in-the-chat)
8. [Rubric mapping (Part 3)](#rubric-mapping-part-3)
9. [AI usage declaration](#ai-usage-declaration)
10. [References](#references)

---

## Features

Combines all three PoE parts in a single WPF application:

| Part | Feature | Where |
|------|---------|-------|
| 1 | Voice greeting (WAV) | `Services/VoiceService.cs` - auto-plays on load |
| 1 | ASCII art banner | `Assets/ascii-banner.txt` - rendered in header |
| 1 | Personalised text greeting | Chat tab |
| 1 | Input validation | `Views/ChatView.xaml.cs`, `TaskView.xaml.cs` |
| 1 | Styled, structured UI | `Resources/Styles.xaml` - dark theme |
| 2 | Keyword recognition (9 topics) | `Services/ChatService.cs` |
| 2 | Random response variation | `Services/ChatService.cs` (multiple replies per topic) |
| 2 | Conversation flow ("tell me more") | `ChatService.TryFollowUp` |
| 2 | Memory & recall (name + favourite topic) | `Models/UserProfile.cs` |
| 2 | Sentiment detection (6 emotions) | `ChatService.TrySentiment` |
| 2 | Generic collections + dictionaries | `ChatService`, `QuizService` |
| 3 | Task assistant with reminders | Tasks tab - `Views/TaskView.xaml` |
| 3 | MySQL database integration (CRUD) | `Services/DatabaseService.cs` |
| 3 | Cybersecurity mini-game (13 questions) | Quiz tab - `Services/QuizService.cs` |
| 3 | NLP intent routing | `Services/NlpService.cs` |
| 3 | Activity log (last 10 + "Show All") | Activity Log tab |

## Requirements

| Tool | Version |
|------|---------|
| Windows | 10 or 11 |
| .NET SDK | 10.0 (preview) - [download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| MySQL Server | 8.0 or later (optional - see below) |

> The app starts even if MySQL isn't installed. The Tasks tab will display the connection status and tasks will be held in memory only. For full marks on the **Database Integration** criterion, install MySQL and follow the setup steps below.

## Setting up MySQL

1. Install MySQL Community Server: <https://dev.mysql.com/downloads/mysql/>
2. From a MySQL shell as `root`, create the database and user:
   ```sql
   CREATE DATABASE cyberbot CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   CREATE USER 'cyberbot'@'localhost' IDENTIFIED BY 'cyberbot';
   GRANT ALL PRIVILEGES ON cyberbot.* TO 'cyberbot'@'localhost';
   FLUSH PRIVILEGES;
   ```
3. Apply the schema:
   ```bash
   mysql -u cyberbot -p cyberbot < db/schema.sql
   ```
4. Confirm the connection string in `appsettings.json` matches your local server:
   ```json
   {
     "Database": {
       "Provider": "MySql",
       "ConnectionString": "Server=localhost;Port=3306;Database=cyberbot;User=cyberbot;Password=cyberbot;SslMode=None;AllowPublicKeyRetrieval=True;"
     }
   }
   ```
The app also calls `CREATE TABLE IF NOT EXISTS` at startup, so step 3 is optional once the database exists.

## Building and running

From the project folder:

```bash
dotnet restore CyberSecurityBot.sln
dotnet build  CyberSecurityBot.sln --configuration Release
dotnet run    --project CyberSecurityBot.csproj
```

Or open `CyberSecurityBot.sln` in Visual Studio 2022/2026 and press F5.

## Running the tests

```bash
dotnet test CyberSecurityBot.sln
```
This runs 16 xUnit tests covering NLP intent detection, the chat service, the quiz, and the activity logger. The same command runs in GitHub Actions on every push (see `.github/workflows/ci.yml`).

## Project layout

```
CyberSecurityBot/
├── App.xaml(.cs)              Application entry point + DI container setup
├── Views/                     WPF user controls and the main window
│   ├── MainWindow.xaml        TabControl shell + ASCII banner header
│   ├── ChatView.xaml          Chat bubbles + input + NLP routing
│   ├── TaskView.xaml          Task CRUD UI bound to MySQL
│   ├── QuizView.xaml          One-question-at-a-time quiz
│   └── ActivityLogView.xaml   Recent + full activity log
├── Models/                    POCOs: CyberTask, QuizQuestion, ChatMessage, ...
├── Services/                  Business logic
│   ├── ChatService.cs         Keyword bank, random replies, memory, sentiment
│   ├── NlpService.cs          Intent classifier, date parsing
│   ├── TaskService.cs         CRUD wrapper over DatabaseService
│   ├── QuizService.cs         13 questions, scoring, feedback
│   ├── ActivityLogger.cs      Singleton log with display limit
│   ├── DatabaseService.cs     MySqlConnector + parameterised SQL
│   └── VoiceService.cs        Plays WAV greeting from resources
├── Resources/Styles.xaml      Dark theme, button/input/tab styles
├── Assets/                    greeting.wav, ascii-banner.txt
├── db/schema.sql              MySQL DDL
├── Tests/                     xUnit test project
└── .github/workflows/ci.yml   Windows-latest, .NET 10, build + test
```

## What to try in the chat

| You type | The bot does |
|----------|--------------|
| `Hi, my name is Mukona` | Remembers your name and personalises replies |
| `Tell me about phishing` | Random phishing tip from the bank |
| `Another one` / `Tell me more` | Continues on the current topic |
| `I'm worried about scams` | Detects sentiment, picks a supportive opener, gives a tip |
| `Add a task to enable two-factor authentication` | Creates a task in MySQL |
| `Remind me to update my password in 3 days` | Creates a reminder dated 3 days from today |
| `Show tasks` | Lists your most recent tasks and switches to the Tasks tab |
| `Start quiz` | Opens the Quiz tab and starts a fresh 13-question quiz |
| `Show activity log` | Switches to the Activity Log tab and prints recent actions |
| `Help` | Lists every topic and command the bot understands |

## Rubric mapping (Part 3)

| Rubric criterion | Implementation |
|---|---|
| Correct Submission (5) | Solution + tests + README + schema + media files all in repo |
| GitHub & Releases with Tags (10) | 6+ commits, releases `v1.0.0-part1-legacy`, `v2.0.0-gui-part2`, `v3.0.0-poe` |
| Task Assistant with Reminders (15) | `TaskView`, `TaskService`, chat-driven creation, reminder DatePicker |
| Task Assistant DB Integration (15) | `DatabaseService.cs` - full CRUD, parameterised SQL, error handling, schema bundled |
| Mini-Game Quiz (15) | `QuizService.cs` (13 questions, MCQ + true/false), `QuizView` with feedback |
| NLP Simulation (10) | `NlpService.cs` - regex + synonyms + date parsing |
| Activity Log (10) | `ActivityLogger.cs`, `ActivityLogView` with "Show All" toggle |
| Combining Parts 1, 2 & 3 (10) | All previous features integrated into the same WPF window |
| Video Presentation (10) | Unlisted YouTube link above |

## AI usage declaration

Per IIE guidelines:

- **Tool:** Google Gemini (Large Language Model).
- **Purpose:** Drafted this README file - the project overview, setup instructions, feature matrix, rubric mapping, and the references list. Also assisted with the ASCII banner concept.
- **Sections affected:** This README.md file and `Assets/ascii-banner.txt`.
- **Date used:** 22 June 2026.
- **Proof of conversation:** <https://gemini.google.com/u/1/app/c3a55b2452b9ce7f?pageId=none>
- All AI-generated content was reviewed and adapted by the author before submission. The application code itself was written by the author.

## References

- GeeksforGeeks. 2024. *C# Properties*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-properties/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Enumeration (or enum)*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-enumeration-or-enum/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# List<T> Class*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-list-class/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Dictionary*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-dictionary/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Switch Statement*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-switch-statement/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Events*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-events/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Regular Expressions*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-regular-expressions/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Tuples*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-sharp-tuples/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *WPF Tutorial*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/wpf-tutorial/> [Accessed 22 June 2026].
- GeeksforGeeks. 2024. *C# Program to Shuffle a List*. [Online] Available at: <https://www.geeksforgeeks.org/c-sharp/c-program-to-shuffle-a-list/> [Accessed 22 June 2026].
- GeeksforGeeks. 2023. *SQL Injection*. [Online] Available at: <https://www.geeksforgeeks.org/sql/sql-injection/> [Accessed 22 June 2026].
- Microsoft. 2026. *Windows Presentation Foundation documentation*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/desktop/wpf/> [Accessed 22 June 2026].
- Microsoft. 2025. *Application Class (System.Windows)*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.windows.application> [Accessed 22 June 2026].
- Microsoft. 2025. *Window Class (System.Windows)*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.windows.window> [Accessed 22 June 2026].
- Microsoft. 2025. *UserControl Class (WPF)*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/usercontrol> [Accessed 22 June 2026].
- Microsoft. 2025. *ObservableCollection<T> Class*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1> [Accessed 22 June 2026].
- Microsoft. 2025. *Configuring Parameters and Parameter Data Types*. [Online] Available at: <https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types> [Accessed 22 June 2026].
- Grainger, B. 2025. *MySqlConnector - ADO.NET Provider for MySQL*. [Online] Available at: <https://mysqlconnector.net/tutorials/connect-to-mysql/> [Accessed 22 June 2026].
- xUnit.net Team. 2025. *xUnit.net testing framework*. [Online] Available at: <https://xunit.net/> [Accessed 22 June 2026].
- Pieterse, H. 2021. *The Cyber Threat Landscape in South Africa: A 10-Year Review*. The African Journal of Information and Communication, 28(28). [Online] Available at: <https://www.scielo.org.za/scielo.php?pid=S2077-72132021000200003&script=sci_arttext> [Accessed 22 June 2026].
- IIE. 2026. *Programming 2A PROG6221/w Module Outline*. The Independent Institute of Education.
