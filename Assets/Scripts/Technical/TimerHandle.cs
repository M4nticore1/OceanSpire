using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHandle
{
    private float time = 0;
    private float delay = 0;
    private Action callBack;
    public bool isFinished { get; private set; } = false;
    public static Action<TimerHandle> OnTimerCreated = null;

    public void SetTimer(float delay, Action callback)
    {
        this.delay = delay;
        this.callBack = callback;
    }

    public void Tick()
    {
        time += Time.deltaTime;
        if (time >= delay)
        {
            callBack?.Invoke();
            isFinished = true;
        }
    }
}
