using UnityEngine;

public class GroundBuildingType : BuildingType
{
    public GroundBuildingType(Building building) : base(building)
    {

    }

    public override bool ShouldBuild()
    {
        return true;
    }
}