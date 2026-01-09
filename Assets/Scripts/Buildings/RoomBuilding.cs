using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/RoomBuilding")]
public class RoomBuilding : TowerBuilding
{
    //protected BuildingPosition buildingPosition = BuildingPosition.Straight;

    public override void BuildConstruction(int levelIndex)
    {
        //RoomLevelData roomLevelData = buildingLevelsData[levelIndex] as RoomLevelData;

        if (currentLevelData is RoomLevelData roomLevelData)
        {
            if (constructionComponent.isUnderConstruction)
            {
                if (buildingPosition == BuildingPosition.Straight)
                {
                    if (roomLevelData.ConstructionStraight)
                        constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                }
                else if (buildingPosition == BuildingPosition.Corner)
                {
                    if (roomLevelData.ConstructionCorner)
                        constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                }
            }
            else
            {
                if (buildingData.ConnectionType == ConnectionType.None)
                {
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Horizontal)
                {
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.ConstructionStraightLeftRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeftRight);
                        else if (leftConnectedBuilding && roomLevelData.ConstructionStraightLeft)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightLeft);
                        else if (rightConnectedBuilding && roomLevelData.ConstructionStraightRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightRight);
                        else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (leftConnectedBuilding && rightConnectedBuilding && roomLevelData.ConstructionCornerLeftRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeftRight);
                        else if (leftConnectedBuilding && roomLevelData.ConstructionCornerLeft)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerLeft);
                        else if (rightConnectedBuilding && roomLevelData.ConstructionCornerRight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerRight);
                        else if (!leftConnectedBuilding && !rightConnectedBuilding && roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
                else if (buildingData.ConnectionType == ConnectionType.Vertical)
                {
                    //Debug.Log(GetfloorIndex + " " + GetplaceIndex);
                    if (buildingPosition == BuildingPosition.Straight)
                    {
                        if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.ConstructionStraightAboveBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAboveBelow);
                        else if (aboveConnectedBuilding && roomLevelData.ConstructionStraightAbove)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightAbove);
                        else if (belowConnectedBuilding && roomLevelData.ConstructionStraightBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraightBelow);
                        else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.ConstructionStraight)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionStraight);
                    }
                    else if (buildingPosition == BuildingPosition.Corner)
                    {
                        if (aboveConnectedBuilding && belowConnectedBuilding && roomLevelData.ConstructionCornerAboveBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAboveBelow);
                        else if (aboveConnectedBuilding && roomLevelData.ConstructionCornerAbove)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerAbove);
                        else if (belowConnectedBuilding && roomLevelData.ConstructionCornerBelow)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCornerBelow);
                        else if (!aboveConnectedBuilding && !belowConnectedBuilding && roomLevelData.ConstructionCorner)
                            constructionComponent.BuildConstruction(roomLevelData.ConstructionCorner);
                    }
                }
            }

            if (isSelected)
                selectComponent.Select();
        }
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);
    }

    private bool GetPossibilityConnect(Building neightboorBuilding, int levelIndex)
    {
        return neightboorBuilding && !neightboorBuilding.constructionComponent.isUnderConstruction && neightboorBuilding.BuildingData.BuildingIdName == buildingData.BuildingIdName && neightboorBuilding.levelIndex == levelIndex;
    }
}
