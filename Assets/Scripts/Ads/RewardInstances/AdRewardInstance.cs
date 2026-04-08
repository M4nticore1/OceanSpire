using UnityEngine;

public abstract class AdRewardInstance
{
    public bool isRemainable { get; private set; } = false;
    public float limitTime { get; private set; } = 0f;
    public float remainingTime { get; private set; } = 0f;

    protected abstract void OnRewardRecieved();

    public void RecieveReward()
    {
        OnRewardRecieved();
        EventBus.InvokeAdRewardRecieved(this);
    }

    public void SetLimitTime(float value)
    {
        limitTime = value;
        remainingTime = value;
    }

    public void ReduceRemainingTime(float value)
    {
        remainingTime -= value;
    }
}