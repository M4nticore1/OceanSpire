using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorldDataMigrator
{
    public static WorldData GetWorldData(string json)
    {
        var jObject = JObject.Parse(json);

        if (!jObject.ContainsKey("Version"))
            throw new Exception("Save file has no Version field");

        int version = jObject["Version"]!.Value<int>();

        switch (version) {
            case 1:
                return JsonConvert.DeserializeObject<WorldData>(json);
        }

        throw new Exception($"Unsupported save version: {version}");
    }
}

[Serializable]
public class WorldData
{
    public int Version = 1;

    public string WorldName = "";

    public PlayerData Player;
    public BuildingData[] GroundBuildings;
    public TowerBuildingData[] FloorFrameBuildings;
    public TowerBuildingData[] TowerBuildings;
    public BoatDockData[] CitizenBoatDocks;
    public BoatDockData[] WandererBoatDocks;
    public BoatDockData[] RaiderBoatDocks;
    public BoatData[] Boats;
    public HumanData[] Citizens;
    public HumanData[] Wanderers;
    public HumanData[] Raiders;
    public ItemData[] Items;
    public DailyTasksData DailyTasks;

    public static WorldData Create(WorldSaveManager saveManager, BuildingsManager buildings, DockPointsManager boatDocks, BoatsManager boats, CreaturesManager creatures, Inventory cityInventory, DailyTasksManager dailyTasks)
    {
        return new WorldData()
        {
            WorldName = saveManager.SaveWorldName,

            GroundBuildings = SaveWorldSystem.SaveGroundBuildings(buildings),
            FloorFrameBuildings = SaveWorldSystem.SaveFloorFrameBuildings(buildings),
            TowerBuildings = SaveWorldSystem.SaveTowerBuildings(buildings),

            CitizenBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.CitizenBoatDocks.ToArray()),
            WandererBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.WandererDockPoints.ToArray()),
            RaiderBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.RaiderDockPoints.ToArray()),

            Boats = SaveWorldSystem.SaveBoats(boats),

            Citizens = SaveWorldSystem.SaveHumans(creatures.Citizens.ToArray()),
            Wanderers = SaveWorldSystem.SaveHumans(creatures.Wanderers.ToArray()),
            Raiders = SaveWorldSystem.SaveHumans(creatures.Raiders.ToArray()),

            Items = SaveWorldSystem.SaveItems(cityInventory),

            DailyTasks = DailyTasksData.Create(dailyTasks),
        };
    }
}

public static class SaveWorldSystem
{
    public static BuildingData[] SaveGroundBuildings(BuildingsManager buildingsManager)
    {
        BuildingData[] buildings = new BuildingData[buildingsManager.GroundBuildings().Count()];

        for (int i = 0; i < buildingsManager.GroundBuildings().Count(); i++) {
            buildings[i] = BuildingData.Create(buildingsManager.GroundBuildings().ToArray()[i]);
        }

        return buildings;
    }

    public static TowerBuildingData[] SaveFloorFrameBuildings(BuildingsManager buildingsManager)
    {
        List<TowerBuildingData> floors = new();

        foreach (var floor in buildingsManager.BuiltFloors) {
            TowerBuilding building = floor.TowerOwnedBuilding;
            if (!building) continue;

            floors.Add(TowerBuildingData.Create(building));
        }

        return floors.ToArray();
    }

    public static TowerBuildingData[] SaveTowerBuildings(BuildingsManager buildingsManager)
    {
        List<TowerBuildingData> buildings = new();

        foreach (var floor in buildingsManager.BuiltFloors) {
            foreach (var place in floor.RoomBuildingPlaces) {
                TowerBuilding building = place.PlacedBuilding;
                if (!building) continue;

                buildings.Add(TowerBuildingData.Create(building));
            }
        }

        return buildings.ToArray();
    }

    public static BoatDockData[] SaveBoatDocks(BoatDockPoint[] boatDocks)
    {
        BoatDockData[] boatDocksData = new BoatDockData[boatDocks.Length];

        for (int i = 0; i < boatDocks.Length; i++) {
            boatDocksData[i] = BoatDockData.Create(boatDocks[i]);
        }

        return boatDocksData;
    }

    public static BoatData[] SaveBoats(BoatsManager boats)
    {
        BoatData[] boatsData = new BoatData[boats.Boats.Count];

        for (int i = 0; i < boats.Boats.Count; i++) {
            boatsData[i] = BoatData.Create(boats.Boats[i]);
        }

        return boatsData;
    }

    public static HumanData[] SaveHumans(Human[] humans)
    {
        HumanData[] result = new HumanData[humans.Length];

        for (int i = 0; i < humans.Length; i++) {
            result[i] = HumanData.Create(humans[i]);
        }

        return result;
    }

    public static ItemData[] SaveItems(Inventory inventory)
    {
        ItemData[] itemsData = new ItemData[inventory.Items.Count];

        for (int i = 0; i < inventory.Items.Count; i++) {
            itemsData[i] = ItemData.Create(inventory.Items[i]);
        }

        return itemsData;
    }
}