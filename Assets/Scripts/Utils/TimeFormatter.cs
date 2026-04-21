using UnityEngine;

public static class TimeFormatter
{
    public static string SecondToTimer(int firstSeconds, int lastSeconds)
    {
        string firstTime = SecondsToMinuteTime(firstSeconds);
        string lastTime = SecondsToMinuteTime(lastSeconds);
        string timer = firstTime + "/" + lastTime;
        return timer;
    }

    public static string SecondsToHourTime(int totalSeconds)
    {
        float hours = (float)totalSeconds / 3600;
        int minutes = (int)((hours - (int)hours) * 60);
        string time = $"{(int)hours:D2}:{minutes:D2}";
        return time;
    }

    public static string SecondsToMinuteTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string time = $"{minutes}:{seconds:D2}";
        return time;
    }
}