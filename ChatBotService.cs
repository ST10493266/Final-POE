using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CybersecurityAwarenessBotGUI;

public sealed class ChatBotService
{
    private readonly TaskRepository _tasks;
    private readonly ActivityLog _activityLog;
    private CyberTask? _pendingReminderTask;

    public ChatBotService(TaskRepository tasks, ActivityLog activityLog)
    {
        _tasks = tasks;
        _activityLog = activityLog;
    }

    public event Action? TasksChanged;
    public event Action? QuizRequested;
    public event Action? LogRequested;

    public string Respond(string input)
    {
        var message = input.Trim();
        var lower = message.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(message)) return "Please type a cybersecurity question or command.";

        if (_pendingReminderTask is not null && IsPositive(lower))
        {
            var reminderDate = ExtractReminderDate(lower) ?? DateTime.Today.AddDays(3);
            _tasks.SetReminder(_pendingReminderTask.Id, reminderDate);
            _activityLog.Add($"Reminder set for '{_pendingReminderTask.Title}' on {reminderDate:yyyy-MM-dd}.");
            TasksChanged?.Invoke();
            var title = _pendingReminderTask.Title;
            _pendingReminderTask = null;
            return $"Got it. I'll remind you about '{title}' on {reminderDate:dddd, dd MMM yyyy}.";
        }

        if (ContainsAny(lower, "show activity", "activity log", "what have you done", "recent actions"))
        {
            _activityLog.Add("Activity log viewed.");
            LogRequested?.Invoke();
            return "Here's a summary of recent actions:
" + FormatActivityLog();
        }

        if (ContainsAny(lower, "quiz", "mini game", "game", "test me"))
        {
            _activityLog.Add("Quiz requested from chat.");
            QuizRequested?.Invoke();
            return "Starting the cybersecurity quiz. Answer each question and I will give feedback immediately.";
        }

        if (ContainsAny(lower, "list tasks", "show tasks", "view tasks", "my tasks"))
        {
            _activityLog.Add("Task list requested from chat.");
            return FormatTasks();
        }

        if (ContainsAny(lower, "remind me", "set reminder", "reminder") || ContainsAny(lower, "add task", "new task", "create task", "task to"))
        {
            return AddTaskFromMessage(message, lower);
        }

        if (ContainsAny(lower, "password")) { _activityLog.Add("NLP response: password safety advice."); return "Use long, unique passwords for each account, and store them in a password manager. Enable two-factor authentication wherever possible."; }
        if (ContainsAny(lower, "phishing", "scam", "suspicious email")) { _activityLog.Add("NLP response: phishing advice."); return "Be careful with urgent messages, unknown links, and requests for passwords. Report suspicious emails as phishing instead of replying."; }
        if (ContainsAny(lower, "privacy", "settings")) { _activityLog.Add("NLP response: privacy advice."); return "Review privacy settings often, limit public information, and check which apps can access your data."; }
        if (ContainsAny(lower, "2fa", "two-factor", "two factor", "authentication")) { _activityLog.Add("NLP response: two-factor authentication advice."); return "Two-factor authentication protects your account by requiring a second verification step after your password."; }

        _activityLog.Add("NLP fallback response.");
        return "I can help with cybersecurity tasks, reminders, password safety, phishing, privacy settings, or the quiz. Try: 'Add a task to enable 2FA tomorrow'.";
    }

    private string AddTaskFromMessage(string original, string lower)
    {
        var reminderDate = ExtractReminderDate(lower);
        var title = ExtractTaskTitle(original);
        var task = _tasks.Add(new CyberTask { Title = title, Description = BuildDescription(title), ReminderDate = reminderDate });
        _activityLog.Add($"Task added: '{task.Title}'{(reminderDate.HasValue ? $" with reminder on {reminderDate:yyyy-MM-dd}" : " without reminder")}.");
        TasksChanged?.Invoke();
        if (reminderDate.HasValue) return $"Task added: '{task.Title}'. Reminder set for {reminderDate:dddd, dd MMM yyyy}.";
        _pendingReminderTask = task;
        return $"Task added with the description "{task.Description}". Would you like a reminder?";
    }

    private string FormatTasks()
    {
        var tasks = _tasks.GetAll();
        if (tasks.Count == 0) return "You do not have any cybersecurity tasks yet.";
        return string.Join("
", tasks.Select(task => $"{task.Id}. {task.Title} - {(task.IsCompleted ? "Completed" : "Pending")}. {(task.ReminderDate.HasValue ? $"Reminder: {task.ReminderDate:yyyy-MM-dd}." : "No reminder.")}"));
    }

    private string FormatActivityLog()
    {
        var entries = _activityLog.Recent(10);
        if (entries.Count == 0) return "No actions have been recorded yet.";
        return string.Join("
", entries.Select((entry, index) => $"{index + 1}. {entry}"));
    }

    private static string ExtractTaskTitle(string message)
    {
        var cleaned = Regex.Replace(message, @"^(pleases+)?(can yous+)?(add|create|set|new)s+(as+)?(task|reminder)s*(to|for|-|:)?", "", RegexOptions.IgnoreCase).Trim();
        cleaned = Regex.Replace(cleaned, @"^(remind mes+(to|about)?s*)", "", RegexOptions.IgnoreCase).Trim();
        cleaned = Regex.Replace(cleaned, @"(today|tomorrow|ins+d+s+days?|nexts+week)", "", RegexOptions.IgnoreCase).Trim();
        cleaned = cleaned.Trim('.', '!', '?', '-', ':', '"');
        if (string.IsNullOrWhiteSpace(cleaned)) return "Review cybersecurity settings";
        return char.ToUpper(cleaned[0]) + cleaned[1..];
    }

    private static string BuildDescription(string title)
    {
        var lower = title.ToLowerInvariant();
        if (lower.Contains("privacy")) return "Review account privacy settings to ensure your data is protected.";
        if (lower.Contains("password")) return "Update and strengthen passwords to protect important accounts.";
        if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor")) return "Enable two-factor authentication for stronger account protection.";
        return $"{title} as part of your cybersecurity safety plan.";
    }

    private static DateTime? ExtractReminderDate(string lower)
    {
        if (lower.Contains("tomorrow")) return DateTime.Today.AddDays(1);
        if (lower.Contains("today")) return DateTime.Today;
        if (lower.Contains("next week")) return DateTime.Today.AddDays(7);
        var match = Regex.Match(lower, @"ins+(d+)s+days?");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var days)) return DateTime.Today.AddDays(days);
        return null;
    }

    private static bool IsPositive(string lower) => ContainsAny(lower, "yes", "sure", "okay", "ok", "please", "remind");
    private static bool ContainsAny(string text, params string[] keywords) => keywords.Any(text.Contains);
}
