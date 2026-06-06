using System;
using System.Collections.Generic;
using UnityEngine;

public class WorkComponent : MonoBehaviour
{
    private List<Citizen> workers = new();
    public IReadOnlyList<Citizen> Workers => workers.AsReadOnly();

    private List<Citizen> enteredWorkers = new();
    public IReadOnlyList<Citizen> EnteredWorkers => enteredWorkers.AsReadOnly();

    public event Action<Citizen> OnWorkerAdded;
    public event Action<Citizen> OnWorkerRemoved;

    public event Action<Citizen> OnWorkerEntered;
    public event Action<Citizen> OnWorkerExited;

    // Workers
    public void AddWorker(Citizen interactor)
    {
        workers.Add(interactor);
        OnWorkerAdded?.Invoke(interactor);
    }

    public void RemoveWorker(Citizen interactor)
    {
        workers.Remove(interactor);
        OnWorkerRemoved?.Invoke(interactor);
    }

    public void AddCurrentWorker(Citizen interactor)
    {
        enteredWorkers.Add(interactor);
        OnWorkerEntered?.Invoke(interactor);
    }

    public void ExitWorker(Citizen interactor)
    {
        enteredWorkers.Remove(interactor);
        OnWorkerExited?.Invoke(interactor);
    }

    public int TryGetIndexOf(Citizen citizen)
    {
        if (!citizen) {
            Debug.Log($"Citizen not found at {name}");
            return 0;
        }

        if (!workers.Contains(citizen)) {
            Debug.Log($"Citizen not found at Workers");
            return 0;
        }

        return workers.IndexOf(citizen);
    }
}