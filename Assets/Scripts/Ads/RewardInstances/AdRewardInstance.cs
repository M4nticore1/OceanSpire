using System;
using System.Collections.Generic;

public abstract class AdRewardInstance : ILocalizable
{
    public AdRewardDefinition definition { get; private set; }
    //public bool isRemainable { get; private set; } = false;
    //public float limitTime { get; private set; } = 0f;
    //public float remainingTime { get; private set; } = 0f;

    private TimerHandle timer = new TimerHandle();

    public static event Action<AdRewardInstance> onRewardRemoved;

    public AdRewardInstance(AdRewardDefinition data, float limitTime)
    {
        definition = data;
        TimerManager.Instance.StartTimer(timer, limitTime, RemoveReward);
    }

    public abstract Dictionary<string, string> GetLocalization();
    protected abstract void OnRewardRecieved();

    public void RecieveReward()
    {
        OnRewardRecieved();
        EventBus.InvokeAdRewardRecieved(this);
    }

    //public void SetLimitTime(float value)
    //{
    //    limitTime = value;
    //    remainingTime = value;
    //}

    //public void ReduceRemainingTime(float value)
    //{
    //    //remainingTime -= value;
    //}

    public float GetLimitTime()
    {
        return timer.delay;
    }

    public float GetRemainingTime()
    {
        return timer.delay - timer.currentTime;
    }

    private void RemoveReward()
    {
        onRewardRemoved?.Invoke(this);
    }
}