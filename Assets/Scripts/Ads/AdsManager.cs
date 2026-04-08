using System;
using UnityEngine;

public abstract class AdsManager : MonoBehaviour
{
    public bool isAdDisplayed { get; private set; } = false;

    public event Action onAdDisplayed;
    public event Action onAdHidden;

    public abstract void ShowAd();

    protected void OnAdDisplayed()
    {
        isAdDisplayed = true;
        onAdDisplayed?.Invoke();
    }

    protected void OnAdHidden()
    {
        isAdDisplayed = false;
        onAdHidden?.Invoke();
    }
}