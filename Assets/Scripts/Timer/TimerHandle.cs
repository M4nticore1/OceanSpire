using System;
using Unity.Mathematics;
using UnityEngine;

public class TimerHandle
{
    public float currentTime = 0;
    public float delay = 0;
    public bool isRunning { get; private set; } = false;
    public static Action<TimerHandle> onTimerCreated = null;
    public float Alpha => delay > 0 ? math.clamp(currentTime / delay, 0f, 1f) : 0f;
    private Action callback;

    public void StartTimer(float delay, Action callback)
    {
        currentTime = 0;
        isRunning = true;
        this.delay = delay;
        this.callback = callback;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = 0;
    }

    public void Tick()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= delay) {
            callback?.Invoke();
            StopTimer();
        }
    }
}
