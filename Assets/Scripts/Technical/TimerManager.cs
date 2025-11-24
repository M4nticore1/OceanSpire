using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager
{
    private static List<TimerHandle> timers = new List<TimerHandle>();

    public static void Start()
    {
        for (int i = 0; i < timers.Count; i++)
        {
            timers[i] = null;
        }
    }

    public static void Tick()
    {
        for (int i = 0; i < timers.Count; i++)
        {
            TimerHandle timer = timers[i];
            timer.Tick();
            if (timer.isFinished)
            {
                timer = null;
                timers.RemoveAt(i);
                i--;
            }
        }
    }

    public static void SetTimer(float delay, Action callback)
    {
        TimerHandle timerHandle = new TimerHandle();
        timerHandle.SetTimer(delay, callback);
        timers.Add(timerHandle);
    }
}
