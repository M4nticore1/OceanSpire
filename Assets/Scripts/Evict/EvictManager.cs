using UnityEngine;

public class EvictManager : MonoBehaviour
{
    [SerializeField] private DockPointsManager boatDocksManager;
    [SerializeField] private Boat evictBoatPrefab;

    public void TryEvict(Citizen citizen)
    {
        if (!ShouldEvict(citizen)) return;

        Evict(citizen);
    }

    private void Evict(Citizen citizen)
    {
        var boat = CreateBoat();
        var leavePosition = WorldUtils.GetRandomBorderPosition();

        var evictData = new EvictData()
        {
            Boat = boat,
            LeavePosition = leavePosition
        };

        citizen.Evict(evictData);
    }

    private Boat CreateBoat()
    {
        var dockPoint = GetNextDockPoint();

        var boatData = new BoatData()
        {
            Id = evictBoatPrefab.Definition.BoatId,
            InstanceId = InstancesManager.Instance.GetNextInstanceId(),
            Position = new Vector3Data(dockPoint.transform.position),
            Rotation = new Vector3Data(dockPoint.transform.rotation.eulerAngles),
            DockInstanceId = dockPoint.InstanceId.GetInstanceId()
        };

        var boat = BoatFactory.CreateBoat(evictBoatPrefab, dockPoint.transform.position, dockPoint.transform.rotation, boatData);

        return boat;
    }

    private BoatDockPoint GetNextDockPoint()
    {
        foreach (var dockPoint in boatDocksManager.EvictDockPoints) {
            if (dockPoint.Boat) continue;

            return dockPoint;
        }

        return boatDocksManager.EvictDockPoints[0];
    }

    private bool ShouldEvict(Citizen citizen)
    {
        if (!citizen) return false;
        if (citizen.IsEvicted) return false;
        if (!citizen.HealthComponent.IsAlive) return false;

        return true;
    }
}