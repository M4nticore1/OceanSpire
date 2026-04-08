using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(CreatureInteractor interactor);
    public void OnCurrentWorkerRemoved(CreatureInteractor interactor);
}
