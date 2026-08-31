using UnityEngine;

public class HallBuildingType : BuildingType
{
    public HallBuildingType(Building building) : base(building)
    {

    }

    public override bool ShouldBuild()
    {
        return true;
    }
}