using UnityEngine;

public class EvictManager : MonoBehaviour
{
    [SerializeField] private BoatDocksManager boatDocksManager;
    [SerializeField] private Boat evictBoatPrefab;

    public void TryEvictCitizen(Citizen citizen)
    {
        if (!ShouldEvict(citizen)) return;

        EvictCitizen(citizen);
    }

    private void EvictCitizen(Citizen citizen)
    {
        if (!citizen) return;

        var boat = CreateBoat();
        if (!boat) {
            Debug.LogError("EvictBoat is not valid");
            return;
        }

        var leavePosition = WorldUtils.GetRandomBorderPosition();
        citizen.Evict(boat, leavePosition);
    }

    private Boat CreateBoat()
    {
        var dockPoint = GetNextDockPoint();
        if (!dockPoint) {
            Debug.Log("EvictDockPoint is not valid");
            return null;
        }

        var boatData = new BoatData()
        {
            Id = evictBoatPrefab.Definition.BoatId,
            Position = new Vector3Data(dockPoint.transform.position),
            Rotation = new Vector3Data(dockPoint.transform.rotation.eulerAngles),
            DockInstanceId = dockPoint.InstanceId.GetGuid(),
            Status = BoatStatusEnum.Evicted
        };

        var boat = BoatFactory.CreateBoat(evictBoatPrefab, boatData);

        return boat;
    }

    private BoatDockPoint GetNextDockPoint()
    {
        foreach (var dockPoint in boatDocksManager.EvictDockPoints) {
            if (dockPoint.Boats.Count > 0) continue;

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