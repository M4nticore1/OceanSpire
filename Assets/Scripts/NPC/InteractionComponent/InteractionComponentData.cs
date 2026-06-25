using System;
using UnityEngine;

public class InteractionComponentData
{
    public Guid? InteractBuildingInstanceId = null;

    public static InteractionComponentData Default()
    {
        return new InteractionComponentData();
    }

    public static InteractionComponentData Create(CreatureInteractComponent interactionComponent)
    {
        return new InteractionComponentData()
        {
            InteractBuildingInstanceId = interactionComponent.InteractBuilding?.InstanceId.GetGuid()
        };
    }
}