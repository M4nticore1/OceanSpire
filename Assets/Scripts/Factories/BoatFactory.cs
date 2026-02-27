using UnityEngine;

public static class BoatFactory
{
    public static Boat CreateBoat(int id, BoatEntry data)
    {
        Boat prefab = BoatsList.Instance.boats[id];
        return CreateBoat_Internal(prefab, data);
    }

    public static Boat CreateBoat(Boat prefab, BoatEntry data)
    {
        return CreateBoat_Internal(prefab, data);
    }

    private static Boat CreateBoat_Internal(Boat prefab, BoatEntry data)
    {
        var obj = Object.Instantiate(prefab);
        obj.Init(data);
        EventBus.InvokeBoatCreated(obj);
        return obj;
    }
}
