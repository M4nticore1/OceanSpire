using UnityEngine;

public class BoatDockPoint : MonoBehaviour
{
    public Boat Boat { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Transform dockTransform;
    public Transform DockTransform => dockTransform;

    [SerializeField] private Transform entranceTransform;
    public Transform EntraceTransform => entranceTransform;

    public void Init(BoatDockData data)
    {
        instanceId.Register(data.InstanceId);
    }

    public void SetBoat(Boat boat)
    {
        this.Boat = boat;
    }

    public void RemoveBoat()
    {
        Boat = null;
    }
}