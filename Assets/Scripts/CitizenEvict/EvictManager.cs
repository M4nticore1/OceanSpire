using UnityEngine;

public class EvictManager : MonoBehaviour
{
    [SerializeField] private Boat evictBoatPrefab;

    [SerializeField] private BoatDockPoint[] evictBoatDockPoints;

    public void TryEvict(Citizen citizen)
    {
        if (!citizen) return;
        if (citizen.HealthComponent.IsAlive) return;

        Evict(citizen);
    }

    private void Evict(Citizen citizen)
    {
        var boat = CreateBoat();

        citizen.Evict(boat);
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
            DockInstanceId = dockPoint.InstanceId.Id
        };

        var boat = BoatFactory.CreateBoat(evictBoatPrefab, dockPoint.transform.position, dockPoint.transform.rotation, boatData);

        return boat;
    }

    private BoatDockPoint GetNextDockPoint()
    {
        foreach (var dockPoint in evictBoatDockPoints) {
            if (dockPoint.Boat) continue;

            return dockPoint;
        }

        return evictBoatDockPoints[0];
    }
}