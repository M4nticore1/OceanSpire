using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(BuildingInteractComponent interactor);
    public void OnCurrentWorkerRemoved(BuildingInteractComponent interactor);
}
