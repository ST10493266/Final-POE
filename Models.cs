using System;
using System.Collections.Generic;

namespace CybersecurityAwarenessBotGUI;

public sealed class CyberTask
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime? ReminderDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class ActivityEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string Description { get; init; } = "";
    public override string ToString() => $"{Timestamp:yyyy-MM-dd HH:mm} - {Description}";
}

public sealed class QuizQuestion
{
    public string Question { get; init; } = "";
    public List<string> Options { get; init; } = new();
    public int CorrectIndex { get; init; }
    public string Explanation { get; init; } = "";
}
