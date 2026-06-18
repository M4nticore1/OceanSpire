using UnityEngine;

public class EvictData
{
    public bool Evicted = false;
    public Vector3Data LeavePosition = Vector3Data.Zero();

    public static EvictData Default()
    {
        return new EvictData();
    }

    public static EvictData Create(Citizen citizen)
    {
        return new EvictData()
        {
            Evicted = citizen.IsEvicted,
            LeavePosition = new Vector3Data(citizen.LeavePosition)
        };
    }
}