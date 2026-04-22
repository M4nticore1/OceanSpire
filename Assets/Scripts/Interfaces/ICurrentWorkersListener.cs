using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(InteractComponent interactor);
    public void OnCurrentWorkerRemoved(InteractComponent interactor);
}
