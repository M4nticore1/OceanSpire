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
    public ElevatorCabinData[] ElevatorCabins;
    public BoatDockData[] CitizenBoatDocks;
    public BoatDockData[] WandererBoatDocks;
    public BoatDockData[] RaiderBoatDocks;
    public BoatData[] Boats;
    public HumanData[] Citizens;
    public WandererData[] Wanderers;
    public RaiderData[] Raiders;
    public ItemData[] CityInventory;
    public DailyTasksData DailyTasks;
    public DailyRewardData DailyReward;
    public RaidData Raid;
    public WanderersData WanderersSystem;

    public static WorldData Create(WorldSaveManager saveManager,
        BuildingsManager buildings,
        ElevatorCabinsManager elevatorCabins,
        DockPointsManager boatDocks,
        BoatsManager boats,
        CreaturesManager creatures,
        Inventory cityInventory,
        DailyTasksManager dailyTasks,
        DailyRewardManager dailyReward,
        RaidManager raid,
        WanderersManager wanderers)
    {
        return new WorldData() {
            WorldName = saveManager.SaveWorldName,

            GroundBuildings = SaveWorldSystem.SaveGroundBuildings(buildings),
            FloorFrameBuildings = SaveWorldSystem.SaveFloorFrameBuildings(buildings),
            TowerBuildings = SaveWorldSystem.SaveTowerBuildings(buildings),
            ElevatorCabins = SaveWorldSystem.SaveElevatorCabins(elevatorCabins),

            CitizenBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.CitizenBoatDocks.ToArray()),
            WandererBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.WandererDockPoints.ToArray()),
            RaiderBoatDocks = SaveWorldSystem.SaveBoatDocks(boatDocks.RaiderDockPoints.ToArray()),

            Boats = SaveWorldSystem.SaveBoats(boats),

            Citizens = SaveWorldSystem.SaveHumans(creatures.Citizens.ToArray()),
            Wanderers = WandererData.Create(creatures.Wanderers.ToArray()),
            Raiders = RaiderData.Create(creatures.Raiders.ToArray()),

            CityInventory = SaveWorldSystem.SaveItems(cityInventory),

            DailyTasks = DailyTasksData.Create(dailyTasks),
            DailyReward = DailyRewardData.Create(dailyReward),
            Raid = RaidData.Create(raid),
            WanderersSystem = WanderersData.Create(wanderers)
        };
    }
}

public static class SaveWorldSystem
{
    public static BuildingData[] SaveGroundBuildings(BuildingsManager buildingsManager)
    {
        var buildings = new BuildingData[buildingsManager.GroundBuildings().Count()];

        for (int i = 0; i < buildingsManager.GroundBuildings().Count(); i++) {
            buildings[i] = BuildingData.Create(buildingsManager.GroundBuildings().ToArray()[i]);
        }

        return buildings;
    }

    public static TowerBuildingData[] SaveFloorFrameBuildings(BuildingsManager buildingsManager)
    {
        List<TowerBuildingData> floors = new();

        foreach (var floor in buildingsManager.BuiltFloors) {
            var building = floor.OwnedTowerBuilding;
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
                var building = place.PlacedBuilding;
                if (!building) continue;

                buildings.Add(TowerBuildingData.Create(building));
            }
        }

        return buildings.ToArray();
    }

    public static ElevatorCabinData[] SaveElevatorCabins(ElevatorCabinsManager elevatorCabinsManager)
    {
        List<ElevatorCabinData> cabins = new();

        foreach (var cabin in elevatorCabinsManager.ElevatorCabins) {
            if (!cabin) continue;

            cabins.Add(ElevatorCabinData.Create(cabin));
        }

        return cabins.ToArray();
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