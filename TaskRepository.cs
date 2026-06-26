using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace CybersecurityAwarenessBotGUI;

public sealed class TaskRepository
{
    private readonly string _connectionString = "server=localhost;port=3306;database=cybersecurity_bot;user=root;password=;";
    public string ConnectionString => _connectionString;

    public void Initialize()
    {
        using var serverConnection = new MySqlConnection("server=localhost;port=3306;user=root;password=;");
        serverConnection.Open();
        using (var createDb = new MySqlCommand("CREATE DATABASE IF NOT EXISTS cybersecurity_bot;", serverConnection))
        {
            createDb.ExecuteNonQuery();
        }

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand(@"
            CREATE TABLE IF NOT EXISTS tasks (
                id INT AUTO_INCREMENT PRIMARY KEY,
                title VARCHAR(200) NOT NULL,
                description TEXT NOT NULL,
                reminder_date DATETIME NULL,
                is_completed BOOLEAN NOT NULL DEFAULT FALSE,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );", connection);
        command.ExecuteNonQuery();
    }

    public List<CyberTask> GetAll()
    {
        var tasks = new List<CyberTask>();
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand(@"
            SELECT id, title, description, reminder_date, is_completed, created_at
            FROM tasks
            ORDER BY is_completed ASC, reminder_date IS NULL ASC, reminder_date ASC, created_at DESC;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new CyberTask
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }
        return tasks;
    }

    public CyberTask Add(CyberTask task)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand(@"
            INSERT INTO tasks (title, description, reminder_date, is_completed, created_at)
            VALUES (@title, @description, @reminderDate, @isCompleted, @createdAt);
            SELECT LAST_INSERT_ID();", connection);
        command.Parameters.AddWithValue("@title", task.Title);
        command.Parameters.AddWithValue("@description", task.Description);
        command.Parameters.AddWithValue("@reminderDate", task.ReminderDate.HasValue ? task.ReminderDate.Value : DBNull.Value);
        command.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
        command.Parameters.AddWithValue("@createdAt", task.CreatedAt);
        task.Id = Convert.ToInt32(command.ExecuteScalar());
        return task;
    }

    public void SetReminder(int taskId, DateTime? reminderDate)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand("UPDATE tasks SET reminder_date = @reminderDate WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", taskId);
        command.Parameters.AddWithValue("@reminderDate", reminderDate.HasValue ? reminderDate.Value : DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void MarkCompleted(int taskId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand("UPDATE tasks SET is_completed = TRUE WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", taskId);
        command.ExecuteNonQuery();
    }

    public void Delete(int taskId)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        using var command = new MySqlCommand("DELETE FROM tasks WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", taskId);
        command.ExecuteNonQuery();
    }
}
