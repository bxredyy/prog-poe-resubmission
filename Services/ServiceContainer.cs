// ServiceContainer.cs
// Builds all the services in one place at startup and wires them together.
// I keep them in one container so MainWindow can hand the same set to every tab.

using CyberSecurityBot.Models;

namespace CyberSecurityBot.Services
{
    public class ServiceContainer
    {
        public UserProfile UserProfile { get; private set; }
        public ActivityLogger ActivityLog { get; private set; }
        public VoiceService Voice { get; private set; }
        public ChatService Chat { get; private set; }
        public TopicCatalogService Catalog { get; private set; }
        public ConversationStateMachine State { get; private set; }
        public CommandService Commands { get; private set; }
        public NlpService Nlp { get; private set; }
        public QuizService Quiz { get; private set; }
        public DatabaseService Database { get; private set; }
        public TaskService Tasks { get; private set; }

        public ServiceContainer()
        {
            // The order matters - each service is built using the ones above it.
            UserProfile = new UserProfile();
            ActivityLog = new ActivityLogger(10);
            Voice = new VoiceService();
            Database = new DatabaseService();
            Tasks = new TaskService(Database, ActivityLog);
            Quiz = new QuizService(ActivityLog);
            Chat = new ChatService(UserProfile, ActivityLog);
            Catalog = new TopicCatalogService();
            State = new ConversationStateMachine();
            Commands = new CommandService(Catalog, State, Chat, ActivityLog, UserProfile);
            Nlp = new NlpService(UserProfile, Tasks, Quiz, Chat, ActivityLog, Catalog, State, Commands);
        }
    }
}
