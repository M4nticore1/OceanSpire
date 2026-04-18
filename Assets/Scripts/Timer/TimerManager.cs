using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static TimerManager instance;
    public static TimerManager Instance => instance;

    private List<TimerHandle> timers = new List<TimerHandle>();

    private void Awake()
    {
        if (instance) {
            Debug.Log("There's an extra TimerManager on the scene!");

            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
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

    public void StartTimer(float delay, Action callback)
    {
        TimerHandle timerHandle = new TimerHandle();
        timerHandle.StartTimer(delay, callback);
        timers.Add(timerHandle);
    }

    public void StartTimer(TimerHandle timerHandle, float delay, Action callback)
    {
        timerHandle.StartTimer(delay, callback);

        if (!timers.Contains(timerHandle)) {
            timers.Add(timerHandle);
        }
    }

    public void RemoveTimer(TimerHandle timerHandle)
    {
        if (!timers.Contains(timerHandle)) return;

        timers.Remove(timerHandle);
        timerHandle.ResetTimer();
    }

    public void ResetTimer(TimerHandle timerHandle)
    {
        if (!timers.Contains(timerHandle)) return;

        timerHandle.ResetTimer();
    }
}
