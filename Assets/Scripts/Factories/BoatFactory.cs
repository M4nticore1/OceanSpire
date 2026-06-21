using UnityEngine;

public static class BoatFactory
{
    public static Boat CreateBoat(Boat prefab, Vector3 position, Quaternion rotation, BoatData data)
    {
        var obj = Object.Instantiate(prefab, position, rotation);
        obj.Init(data);

        return obj;
    }
}