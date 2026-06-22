using UnityEngine;

public static class BoatDockUtils
{
    public static BoatDockPoint GetNearestFreeDockPoint(BoatDockPoint[] boatDocks, Vector3 position)
    {
        BoatDockPoint bestDockPoint = null;
        float bestSqr = float.MaxValue;

        BoatDockPoint leastBusyDockPoint = null;
        int minBoatCount = int.MaxValue;
        float leastBusyBestSqr = float.MaxValue;

        for (int i = 0; i < boatDocks.Length; i++) {
            var dockPoint = boatDocks[i];
            int currentBoatCount = dockPoint.Boats.Count;
            float sqr = (position - dockPoint.transform.position).sqrMagnitude;

            if (currentBoatCount == 0) {
                if (sqr < bestSqr) {
                    bestDockPoint = dockPoint;
                    bestSqr = sqr;
                }
            }
            else if (bestDockPoint == null) {
                if (currentBoatCount < minBoatCount) {
                    minBoatCount = currentBoatCount;
                    leastBusyDockPoint = dockPoint;
                    leastBusyBestSqr = sqr;
                }
                else if (currentBoatCount == minBoatCount) {
                    if (sqr < leastBusyBestSqr) {
                        leastBusyDockPoint = dockPoint;
                        leastBusyBestSqr = sqr;
                    }
                }
            }
        }

        return bestDockPoint ?? leastBusyDockPoint;
    }
}