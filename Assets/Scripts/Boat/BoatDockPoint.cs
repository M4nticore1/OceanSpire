using UnityEngine;

public class BoatDockData
{
    public int instanceId { get; private set; } = 0;
}

public class BoatDockPoint : MonoBehaviour
{
    public Boat boat { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Transform dockTransform;
    public Transform DockTransform => dockTransform;

    [SerializeField] private Transform entranceTransform;
    public Transform EntraceTransform => entranceTransform;

    public void Init()
    {
        instanceId.Init();
        DockPointsManager.instance.RegisterDockPoint(this);
    }

    public void SetBoat(Boat boat)
    {
        this.boat = boat;
    }

    public void RemoveBoat()
    {
        boat = null;
    }
}