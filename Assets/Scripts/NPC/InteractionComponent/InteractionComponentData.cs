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
        var interactBuilding = interactionComponent.InteractBuilding;

        return new InteractionComponentData()
        {
            InteractBuildingInstanceId = interactBuilding ? interactBuilding.InstanceId.GetGuid() : null
        };
    }
}