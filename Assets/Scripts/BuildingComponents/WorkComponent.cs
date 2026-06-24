using System;
using System.Collections.Generic;
using UnityEngine;

public class WorkComponent : MonoBehaviour
{
    private List<Citizen> workers = new();
    public IReadOnlyList<Citizen> Workers => workers.AsReadOnly();

    private List<Citizen> currentWorkers = new();
    public IReadOnlyList<Citizen> CurrentWorkers => currentWorkers.AsReadOnly();

    public event Action<Citizen> OnWorkerAdded;
    public event Action<Citizen> OnWorkerRemoved;

    public event Action<Citizen> OnCurrentWorkerAdded;
    public event Action<Citizen> OnCurrentWorkerRemoved;

    public static event Action<WorkComponent, Citizen> OnComponentWorkerAdded;
    public static event Action<WorkComponent, Citizen> OnComponentWorkerRemoved;

    public static event Action<WorkComponent, Citizen> OnComponentCurrentWorkerAdded;
    public static event Action<WorkComponent, Citizen> OnComponentCurrentWorkerRemoved;

    // Workers
    public void AddWorker(Citizen interactor)
    {
        workers.Add(interactor);
        OnWorkerAdded?.Invoke(interactor);
        OnComponentWorkerAdded?.Invoke(this, interactor);
    }

    public void RemoveWorker(Citizen interactor)
    {
        workers.Remove(interactor);
        OnWorkerRemoved?.Invoke(interactor);
        OnComponentWorkerRemoved?.Invoke(this, interactor);
    }

    public void AddCurrentWorker(Citizen interactor)
    {
        currentWorkers.Add(interactor);
        OnCurrentWorkerAdded?.Invoke(interactor);
        OnComponentCurrentWorkerAdded?.Invoke(this, interactor);
    }

    public void RemoveCurrentWorker(Citizen interactor)
    {
        currentWorkers.Remove(interactor);
        OnCurrentWorkerRemoved?.Invoke(interactor);
        OnComponentCurrentWorkerRemoved?.Invoke(this, interactor);
    }

    public int? TryGetWorkerIndex(Citizen citizen)
    {
        if (!citizen) {
            Debug.Log($"Citizen not found at {name}");
            return null;
        }

        if (!workers.Contains(citizen)) {
            Debug.Log($"Citizen not found at Workers");
            return null;
        }

        return workers.IndexOf(citizen);
    }
}