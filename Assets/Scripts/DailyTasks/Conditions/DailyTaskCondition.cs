using System;
using UnityEngine;

public abstract class DailyTaskCondition : MonoBehaviour
{
    [Header("Task Condition")]
    [SerializeField] private DailyTaskDefinition[] definitions;
    public DailyTaskDefinition[] Definitions => definitions;

    private bool isSubscribed = false;

    public static event Action<DailyTaskCondition, int> OnProgressChanged;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    protected abstract bool Subscribe();
    protected abstract bool Unsubscribe();

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!Subscribe()) return;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!Unsubscribe()) return;

        isSubscribed = false;
    }

    protected void InvokeProgressChanged(int value)
    {
        OnProgressChanged?.Invoke(this, value);
    }
}
