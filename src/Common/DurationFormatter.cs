namespace PioneerConverter.Core.Common;

public static class DurationFormatter
{
    public static string Format(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
        {
            return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m {elapsed.Seconds}s";
        }

        if (elapsed.TotalMinutes >= 1)
        {
            return $"{elapsed.Minutes}m {elapsed.Seconds}s";
        }

        if (elapsed.TotalSeconds >= 1)
        {
            return $"{elapsed.TotalSeconds:F1}s";
        }

        return $"{elapsed.TotalMilliseconds:F0}ms";
    }
}
