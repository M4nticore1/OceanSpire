using UnityEngine;
using System;
using System.Collections.Generic;

public class TimerManager
{
    public TimerManager()
    {
        for (int i = 0; i < timers.Count; i++) {
            timers[i] = null;
        }
        timers.Clear();
    }

    private static List<TimerHandle> timers = new List<TimerHandle>();

    public static void Tick() // Run it from only one class in the update function.
    {
        for (int i = 0; i < timers.Count; i++) {
            TimerHandle timer = timers[i];
            timer.Tick();

            if (!timer.isRunning) {
                timer = null;
                timers.RemoveAt(i);
                i--;
            }
        }
    }

    public static void StartTimer(float delay, Action callback)
    {
        TimerHandle timerHandle = new TimerHandle();
        timerHandle.StartTimer(delay, callback);
        timers.Add(timerHandle);
    }

    public static void StartTimer(TimerHandle timerHandle, float delay, Action callback)
    {
        timerHandle.StartTimer(delay, callback);

        if (!timers.Contains(timerHandle)) {
            timers.Add(timerHandle);
        }
    }

    public static void RemoveTimer(TimerHandle timerHandle)
    {
        if (!timers.Contains(timerHandle)) return;

        timers.Remove(timerHandle);
        timerHandle.ResetTimer();
    }

    public static void ResetTimer(TimerHandle timerHandle)
    {
        if (!timers.Contains(timerHandle)) return;

        timerHandle.ResetTimer();
    }
}
