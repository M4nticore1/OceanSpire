using UnityEngine;

public static class BoatFactory
{
    public static Boat CreateBoat(Boat prefab, BoatData data)
    {
        var obj = Object.Instantiate(prefab, data.Position.Vector3(), Quaternion.Euler(data.Rotation.Vector3()));
        obj.Init(data);

        return obj;
    }
}