using UnityEngine;

public static class BoatFactory
{
    public static Boat CreateBoat(Boat prefab, BoatEntry data)
    {
        var obj = Object.Instantiate(prefab);
        obj.Init(data);

        EventBus.InvokeBoatCreated(obj);
        return obj;
    }
}