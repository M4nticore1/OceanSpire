using UnityEngine;

public static class TimeFormatter
{
    public static string SecondToTimer(int firstSeconds, int lastSeconds)
    {
        string firstTime = SecondsToTime(firstSeconds);
        string lastTime = SecondsToTime(lastSeconds);
        string timer = firstTime + "/" + lastTime;
        return timer;
    }

    public static string SecondsToTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string time = $"{minutes}:{seconds:D2}";
        return time;
    }
}
