using System;
using System.Collections.Generic;
using UnityEngine;

public class WorkComponent : MonoBehaviour
{
    private List<InteractComponent> workers = new List<InteractComponent>();
    public IReadOnlyList<InteractComponent> Workers => workers.AsReadOnly();

    private List<InteractComponent> enteredWorkers = new List<InteractComponent>();
    public IReadOnlyList<InteractComponent> EnteredWorkers => enteredWorkers.AsReadOnly();

    public event Action<InteractComponent> onWorkerAdded;
    public event Action<InteractComponent> onWorkerRemoved;

    public event Action<InteractComponent> onWorkerEntered;
    public event Action<InteractComponent> onWorkerExited;

    // Workers
    public void AddWorker(InteractComponent interactor)
    {
        workers.Add(interactor);
        onWorkerAdded?.Invoke(interactor);
    }

    public void RemoveWorker(InteractComponent interactor)
    {
        workers.Remove(interactor);
        onWorkerRemoved?.Invoke(interactor);
    }

    public void EnterWorker(InteractComponent interactor)
    {
        enteredWorkers.Add(interactor);
        onWorkerEntered?.Invoke(interactor);
    }

    public void ExitWorker(InteractComponent interactor)
    {
        enteredWorkers.Remove(interactor);
        onWorkerExited?.Invoke(interactor);
    }
}