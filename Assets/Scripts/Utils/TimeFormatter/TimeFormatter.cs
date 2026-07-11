using UnityEngine;

public static class TimeFormatter
{
    private static TimeSymbols symbols;

    public static string SecondToFractionalTimer(int firstSeconds, int lastSeconds)
    {
        string firstTime = SecondsToTimer(firstSeconds);
        string lastTime = SecondsToTimer(lastSeconds);
        string timer = firstTime + "/" + lastTime;

        return timer;
    }

    public static string SecondsToTimer(int totalSeconds)
    {
        if (totalSeconds >= 3600) {
            return SecondsToHourTimer(totalSeconds);
        }
        else if (totalSeconds >= 60) {
            return SecondsToMinuteTimer(totalSeconds);
        }
        else {
            return SecondsToSecondTimer(totalSeconds);
        }
    }

    public static string SecondsToHourTimer(int totalSeconds)
    {
        float hours = (float)totalSeconds / 3600;
        int minutes = (int)((hours - (int)hours) * 60);
        string time = $"{(int)hours}{symbols.Hour} {minutes}{symbols.Minute}";

        return time;
    }

    public static string SecondsToMinuteTimer(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string time = $"{minutes}{symbols.Minute} {seconds}{symbols.Second}";

        return time;
    }

    public static string SecondsToSecondTimer(int totalSeconds)
    {
        int seconds = totalSeconds % 60;
        string time = $"{seconds}{symbols.Second}";

        return time;
    }

    public static void SetSymbols(TimeSymbols newSymbols)
    {
        symbols = newSymbols;
    }
}