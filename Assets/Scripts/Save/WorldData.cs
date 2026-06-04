using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;

public static class WorldDataMigrator
{
    public static WorldData GetWorldData(string json)
    {
        try {
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
        catch (Exception ex) {
            Console.WriteLine(ex);
            return null;
        }
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
    public BoatDockData[] EvictBoatDocks;
    public BoatData[] Boats;
    public CitizenData[] Citizens;
    public WandererData[] Wanderers;
    public RaiderData[] Raiders;
    public DriftingLootSystemData DriftingLoot;
    public ItemData[] CityInventory;
    public DailyTasksData DailyTasks;
    public DailyRewardData DailyReward;
    public RaidData Raid;
    public WanderersData WanderersSystem;
    public TutorialData Tutorial;
    public WindData Wind;

    public static WorldData Create(WorldSaveManager saveManager,
        BuildingsManager buildings,
        ElevatorCabinsManager elevatorCabins,
        DockPointsManager boatDocks,
        BoatsManager boats,
        CreaturesManager creatures,
        DriftingLootManager driftingLoot,
        Inventory cityInventory,
        DailyTasksManager dailyTasks,
        DailyRewardManager dailyReward,
        RaidManager raid,
        WanderersManager wanderers,
        TutorialManager tutorial,
        WindManager wind)
    {
        return new WorldData() {
            WorldName = saveManager.SaveWorldName,

            GroundBuildings = BuildingData.Create(buildings.GroundBuildings().ToArray()),
            FloorFrameBuildings = TowerBuildingData.Create(buildings.BuiltFloors.Select(b => b.OwnedTowerBuilding).ToArray()),
            TowerBuildings = TowerBuildingData.Create(buildings.BuiltFloors.SelectMany(b => b.RoomBuildingPlaces).Select(p => p.PlacedBuilding).Where(b => b != null).ToArray()),
            ElevatorCabins = ElevatorCabinData.Create(elevatorCabins.ElevatorCabins.ToArray()),

            CitizenBoatDocks = BoatDockData.Create(boatDocks.CitizenBoatDocks.ToArray()),
            WandererBoatDocks = BoatDockData.Create(boatDocks.WandererDockPoints.ToArray()),
            RaiderBoatDocks = BoatDockData.Create(boatDocks.RaiderDockPoints.ToArray()),
            EvictBoatDocks = BoatDockData.Create(boatDocks.EvictDockPoints.ToArray()),

            Boats = BoatData.Create(boats.Boats.Values.ToArray()),

            Citizens = CitizenData.Create(creatures.Citizens.ToArray()),
            Wanderers = WandererData.Create(creatures.Wanderers.ToArray()),
            Raiders = RaiderData.Create(creatures.Raiders.ToArray()),

            DriftingLoot = DriftingLootSystemData.Create(driftingLoot),
            CityInventory = ItemData.Create(cityInventory.Items.ToArray()),

            DailyTasks = DailyTasksData.Create(dailyTasks),
            DailyReward = DailyRewardData.Create(dailyReward),
            Raid = RaidData.Create(raid),
            WanderersSystem = WanderersData.Create(wanderers),
            Tutorial = TutorialData.Create(tutorial),
            Wind = WindData.Create(wind)
        };
    }
}