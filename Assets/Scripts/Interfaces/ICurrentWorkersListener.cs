using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(EntityInteractor interactor);
    public void OnCurrentWorkerRemoved(EntityInteractor interactor);
}
