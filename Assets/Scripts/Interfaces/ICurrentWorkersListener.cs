using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(BuildingInteractHandler interactor);
    public void OnCurrentWorkerRemoved(BuildingInteractHandler interactor);
}
