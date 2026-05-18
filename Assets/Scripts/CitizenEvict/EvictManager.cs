using UnityEngine;

public class EvictManager : MonoBehaviour
{
    [SerializeField] private Boat evictBoatPrefab;

    [SerializeField] private BoatDockPoint[] evictBoatDockPoints;

    public void Evict(Citizen citizen)
    {
        var dockPoint = GetNextDockPoint();

        var boatData = new BoatData()
        {

        };

        var boat = BoatFactory.CreateBoat(evictBoatPrefab, dockPoint.transform.position, dockPoint.transform.rotation, boatData);

        citizen.Evict();
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