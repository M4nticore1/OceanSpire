using UnityEngine;

public static class BoatFactory
{
    public static Boat CreateBoat(Boat prefab, BoatEntry saveData)
    {
        var obj = Object.Instantiate(prefab);
        obj.Init(saveData);
        return obj;
    }
}
