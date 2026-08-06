using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Session-wide record of quest markers. Quest markers register themselves and
/// report completion here, so UI in other scenes (e.g. the map button) can tell
/// when every quest is finished. Static state resets each play session, so a new
/// playthrough always starts with all quests unfinished.
/// </summary>
public static class QuestManager
{
    static readonly HashSet<string> registered = new HashSet<string>();
    static readonly HashSet<string> completed = new HashSet<string>();

    public static void Register(string id)
    {
        if (!string.IsNullOrEmpty(id)) registered.Add(id);
    }

    public static void Complete(string id)
    {
        if (!string.IsNullOrEmpty(id)) completed.Add(id);
    }

    public static bool IsComplete(string id) => completed.Contains(id);

    /// <summary>Quests seen but not yet completed.</summary>
    public static int RemainingCount() => registered.Count(id => !completed.Contains(id));

    /// <summary>True once at least one quest has been seen and none remain.</summary>
    public static bool AllComplete() => registered.Count > 0 && RemainingCount() == 0;
}
