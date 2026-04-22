using System;
using UnityEngine;

public abstract class AdsSystem : MonoBehaviour
{
    public bool isAdDisplayed { get; private set; } = false;

    public event Action onAdStarted;
    public event Action onAdCompleted;

    public abstract void ShowAd();

    protected void OnAdStarted()
    {
        isAdDisplayed = true;
        onAdStarted?.Invoke();
    }

    protected void OnAdCompleted()
    {
        isAdDisplayed = false;
        onAdCompleted?.Invoke();
    }
}