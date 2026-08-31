using UnityEngine;

public abstract class BuildingType
{
    public Building Building { get; private set; }
    public TowerBuilding TowerBuilding => Building as TowerBuilding;
    public GroundBuilding GroundBuilding => Building as GroundBuilding;

    public BuildingType(Building building)
    {
        Building = building;
    }

    public abstract bool ShouldBuild();

    public virtual bool ShouldBuild(BuildingPlace buildingPlace)
    {
        if (buildingPlace == null) return false;

        return true;
    }
}