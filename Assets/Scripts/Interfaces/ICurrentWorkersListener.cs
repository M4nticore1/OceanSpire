using UnityEngine;

public interface ICurrentWorkersListener
{
    public void OnCurrentWorkerAdded(CreatureInteractComponent interactor);
    public void OnCurrentWorkerRemoved(CreatureInteractComponent interactor);
}
