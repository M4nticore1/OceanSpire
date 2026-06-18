using UnityEngine;

public class InteractionComponentData
{
    public int? InteractBuildingInstanceId = null;

    public static InteractionComponentData Default()
    {
        return new InteractionComponentData();
    }

    public static InteractionComponentData Create(CreatureInteractComponent interactionComponent)
    {
        return new InteractionComponentData()
        {
            InteractBuildingInstanceId = interactionComponent.InteractBuilding?.InstanceId.GetId()
        };
    }
}