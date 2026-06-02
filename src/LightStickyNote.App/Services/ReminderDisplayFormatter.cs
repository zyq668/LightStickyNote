namespace LightStickyNote.App.Services;

public static class ReminderDisplayFormatter
{
    public static string FormatBadge(DateTimeOffset reminderAt, DateTimeOffset now)
    {
        if (reminderAt <= now)
        {
            return "已逾期";
        }

        var reminderLocal = reminderAt.LocalDateTime;
        var nowLocal = now.LocalDateTime;

        if (reminderLocal.Date == nowLocal.Date)
        {
            return $"今天 {reminderLocal:HH:mm}";
        }

        if (reminderLocal.Date == nowLocal.Date.AddDays(1))
        {
            return $"明天 {reminderLocal:HH:mm}";
        }

        return $"{reminderLocal:M/d HH:mm}";
    }
}
