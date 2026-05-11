using UnityEngine;

public class BoatDockPoint : MonoBehaviour
{
    public Boat boat { get; private set; }

    [SerializeField] private InstanceId instanceId;
    public InstanceId InstanceId => instanceId;

    [SerializeField] private Transform dockTransform;
    public Transform DockTransform => dockTransform;

    [SerializeField] private Transform entranceTransform;
    public Transform EntraceTransform => entranceTransform;

    private bool isInited = false;

    private void Start()
    {
        if (isInited) return;

        BoatDockData dockData = new BoatDockData()
        {
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
        };

        Init(dockData);
    }

    public void Init(BoatDockData data)
    {
        instanceId.Init(data.InstanceId);

        isInited = true;
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