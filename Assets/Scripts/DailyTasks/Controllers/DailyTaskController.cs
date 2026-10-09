using System;
using UnityEngine;

public abstract class DailyTaskController : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private DailyTaskDefinition dailyTaskDefinition;
    public DailyTaskDefinition DailyTaskDefinition => dailyTaskDefinition;

    private bool isSubscribed = false;

    public static event Action<DailyTaskController, int> OnProgressChanged;

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

    protected void AddTaskProgress(int value)
    {
        OnProgressChanged?.Invoke(this, value);
    }
}
