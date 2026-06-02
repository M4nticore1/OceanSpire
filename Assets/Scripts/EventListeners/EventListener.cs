using System;
using UnityEngine;

public abstract class EventListener : MonoBehaviour
{
    public event Action OnTriggered;

    private void Start()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    protected void HandleTriggered()
    {
        OnTriggered?.Invoke();
    }
}