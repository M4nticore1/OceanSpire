using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TimerHandle
{
    private float currentTime = 0;
    private float delay = 0;
    private Action callBack;
    public bool isFinished { get; private set; } = false;
    public static Action<TimerHandle> OnTimerCreated = null;
    public float alpha { get { return delay > 0 ? math.clamp(currentTime / delay, 0f, 1f) : 0f; } }

    public void SetTimer(float delay, Action callback)
    {
        currentTime = 0;
        isFinished = false;
        this.delay = delay;
        this.callBack = callback;
    }

    public void Tick()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= delay)
        {
            callBack?.Invoke();
            isFinished = true;
        }
    }
}
