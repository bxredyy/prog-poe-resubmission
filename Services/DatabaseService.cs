// DatabaseService.cs
// Talks to MySQL. Does all the CRUD (Create/Read/Update/Delete) for tasks.
// Uses parameters in every SQL statement so user input cannot be used for SQL injection.
// If MySQL is not running it still works - the app falls back to memory only.
// POE Part 3 Task 1: Database Integration.
//
// References:
//   MySqlConnector. ADO.NET Provider for MySQL.
//     https://mysqlconnector.net/tutorials/connect-to-mysql/
//   GeeksforGeeks. SQL Injection.
//     https://www.geeksforgeeks.org/sql/sql-injection/
//   Microsoft Docs. Parameterized SQL queries.
//     https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/configuring-parameters-and-parameter-data-types

using System;
using System.Collections.Generic;
using CyberSecurityBot.Models;
using MySqlConnector;

namespace CyberSecurityBot.Services
{
    public class DatabaseService
    {
        // Hardcoded for the school demo. In a real app this would come from a config file.
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=cyberbot;User=cyberbot;Password=cyberbot;SslMode=None;AllowPublicKeyRetrieval=True;";

        public bool IsAvailable { get; private set; }
        public string Status { get; private set; } = "Unknown";

        public DatabaseService()
        {
            ProbeConnection();
            if (IsAvailable) EnsureSchema();
        }

        // Try to open a connection so we know up front whether MySQL is reachable.
        private void ProbeConnection()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(ConnectionString);
                conn.Open();
                conn.Close();
                IsAvailable = true;
                Status = "Connected to MySQL.";
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                Status = "MySQL unavailable (" + ex.Message + "). Tasks will run in memory-only mode.";
            }
        }

        // Create the tables on first run if they don't exist yet.
        private void EnsureSchema()
        {
            string sql =
                "CREATE TABLE IF NOT EXISTS tasks (" +
                "  id INT PRIMARY KEY AUTO_INCREMENT," +
                "  title VARCHAR(200) NOT NULL," +
                "  description TEXT," +
                "  status VARCHAR(20) NOT NULL DEFAULT 'Pending'," +
                "  reminder_at DATETIME NULL," +
                "  created_at DATETIME NOT NULL," +
                "  completed_at DATETIME NULL" +
                ");" +
                "CREATE TABLE IF NOT EXISTS activity_log (" +
                "  id INT PRIMARY KEY AUTO_INCREMENT," +
                "  ts DATETIME NOT NULL," +
                "  category VARCHAR(40) NOT NULL," +
                "  description VARCHAR(500) NOT NULL" +
                ");" +
                "CREATE TABLE IF NOT EXISTS quiz_attempts (" +
                "  id INT PRIMARY KEY AUTO_INCREMENT," +
                "  score INT NOT NULL," +
                "  total INT NOT NULL," +
                "  completed_at DATETIME NOT NULL" +
                ");";
            try
            {
                MySqlConnection conn = new MySqlConnection(ConnectionString);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                Status = "Schema setup failed: " + ex.Message;
            }
        }

        // Add a new task. Returns the new auto-incremented id.
        public int InsertTask(CyberTask task)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(
                "INSERT INTO tasks (title, description, status, reminder_at, created_at) " +
                "VALUES (@title, @desc, @status, @reminder, @created); " +
                "SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@desc", task.Description == null ? "" : task.Description);
            cmd.Parameters.AddWithValue("@status", task.Status.ToString());
            if (task.ReminderAt.HasValue)
                cmd.Parameters.AddWithValue("@reminder", task.ReminderAt.Value);
            else
                cmd.Parameters.AddWithValue("@reminder", DBNull.Value);
            cmd.Parameters.AddWithValue("@created", task.CreatedAt);
            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            return newId;
        }

        // Read every task in the database, newest first.
        public List<CyberTask> GetAllTasks()
        {
            List<CyberTask> list = new List<CyberTask>();
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(
                "SELECT id, title, description, status, reminder_at, created_at, completed_at " +
                "FROM tasks ORDER BY created_at DESC;", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CyberTask t = new CyberTask();
                t.Id = reader.GetInt32(0);
                t.Title = reader.GetString(1);
                t.Description = reader.IsDBNull(2) ? "" : reader.GetString(2);

                CyberTaskStatus parsed;
                if (Enum.TryParse<CyberTaskStatus>(reader.GetString(3), out parsed))
                    t.Status = parsed;
                else
                    t.Status = CyberTaskStatus.Pending;

                if (!reader.IsDBNull(4)) t.ReminderAt = reader.GetDateTime(4);
                t.CreatedAt = reader.GetDateTime(5);
                if (!reader.IsDBNull(6)) t.CompletedAt = reader.GetDateTime(6);
                list.Add(t);
            }
            reader.Close();
            conn.Close();
            return list;
        }

        // Mark a task as done (or back to pending).
        public void UpdateTaskStatus(int id, CyberTaskStatus status)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(
                "UPDATE tasks SET status = @status, completed_at = @completed WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@status", status.ToString());
            if (status == CyberTaskStatus.Done)
                cmd.Parameters.AddWithValue("@completed", DateTime.Now);
            else
                cmd.Parameters.AddWithValue("@completed", DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // Delete a task by id.
        public void DeleteTask(int id)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand("DELETE FROM tasks WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // Update the title, description and reminder for a task.
        public void UpdateTask(CyberTask task)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(
                "UPDATE tasks SET title = @title, description = @desc, reminder_at = @reminder " +
                "WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@title", task.Title);
            cmd.Parameters.AddWithValue("@desc", task.Description == null ? "" : task.Description);
            if (task.ReminderAt.HasValue)
                cmd.Parameters.AddWithValue("@reminder", task.ReminderAt.Value);
            else
                cmd.Parameters.AddWithValue("@reminder", DBNull.Value);
            cmd.Parameters.AddWithValue("@id", task.Id);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // Save an activity log line. Wrapped in try/catch so logging never crashes the app.
        public void InsertActivity(ActivityLogEntry entry)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(ConnectionString);
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "INSERT INTO activity_log (ts, category, description) VALUES (@ts, @cat, @desc);", conn);
                cmd.Parameters.AddWithValue("@ts", entry.Timestamp);
                cmd.Parameters.AddWithValue("@cat", entry.Category);
                cmd.Parameters.AddWithValue("@desc", entry.Description);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch
            {
                // logging never breaks the app
            }
        }

        // Save the final quiz score when a quiz finishes.
        public void InsertQuizAttempt(int score, int total)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(
                "INSERT INTO quiz_attempts (score, total, completed_at) VALUES (@s, @t, @d);", conn);
            cmd.Parameters.AddWithValue("@s", score);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.Parameters.AddWithValue("@d", DateTime.Now);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
