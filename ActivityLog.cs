using System.Collections.Generic;
using System.Linq;

namespace CybersecurityAwarenessBotGUI;

public sealed class ActivityLog
{
    private readonly List<ActivityEntry> _entries = new();

    public void Add(string description) => _entries.Add(new ActivityEntry { Description = description });

    public IReadOnlyList<ActivityEntry> Recent(int count = 10)
    {
        return _entries.OrderByDescending(entry => entry.Timestamp).Take(count).ToList();
    }
}
