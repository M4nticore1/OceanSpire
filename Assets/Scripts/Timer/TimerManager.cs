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
            var timer = timers[i];
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
        var timerHandle = new TimerHandle();
        timerHandle.StartTimer(delay, callback);
        timers.Add(timerHandle);
    }

    public void StartTimer(TimerHandle timerHandle, float delay, Action callback)
    {
        if (timerHandle == null) {
            Debug.LogError($"[{nameof(TimerManager)}] Timer Handle is not valid!");
            return;
        }

        timerHandle.StartTimer(delay, callback);

        if (!timers.Contains(timerHandle)) {
            timers.Add(timerHandle);
        }
    }

    public void RemoveTimer(TimerHandle timerHandle)
    {
        if (timerHandle == null) return;
        if (!timers.Contains(timerHandle)) return;

        timers.Remove(timerHandle);
        timerHandle.ResetTimer();
    }

    public void ResetTimer(TimerHandle timerHandle)
    {
        if (timerHandle == null) return;
        if (!timers.Contains(timerHandle)) return;

        timerHandle.ResetTimer();
    }
}
