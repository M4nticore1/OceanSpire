using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public long SaveTime = 0;

    public PlayerControllerData Player;
    public List<BuildingData> GroundBuildings;
    public List<TowerBuildingData> FloorFrameBuildings;
    public List<TowerBuildingData> TowerBuildings;
    public List<ElevatorCabinData> ElevatorCabins;
    public List<BoatDockData> CitizenBoatDocks;
    public List<BoatDockData> WandererBoatDocks;
    public List<BoatDockData> RaiderBoatDocks;
    public List<BoatDockData> EvictBoatDocks;
    public List<BoatData> Boats;
    public List<CitizenData> Citizens;
    public List<WandererData> Wanderers;
    public List<RaiderData> Raiders;
    public DriftingLootSystemData DriftingLoot;
    public FocusSystemData FocusSystem;
    public InventoryData CityStorage;
    public DailyTasksData DailyTasks;
    public DailyRewardData DailyReward;
    public RaidData Raid;
    public WandererSystemData WanderersSystem;
    public BuilderEnergyData BuilderEnergy;
    public ReviveSystemData ReviveSystem;
    public FoodDrainData FoodDrain;
    public WindData Wind;

    public static WorldData Create(WorldSaveHandler saveManager,
        PlayerController playerController,
        BuildingsManager buildings,
        ElevatorCabinsManager elevatorCabins,
        BoatDocksManager boatDocks,
        BoatsManager boats,
        CreaturesManager creatures,
        DriftingLootManager driftingLoot,
        LootContainersList driftingLootList,
        Inventory cityInventory,
        DailyTasksManager dailyTasks,
        DailyRewardManager dailyReward,
        RaidManager raid,
        WanderersManager wanderers,
        BuilderEnergyManager constructionEnergy,
        ReviveManager revive,
        FocusManager focusManager,
        FoodDrainManager foodDtain,
        WindManager wind)
    {
        return new WorldData() {
            WorldName = saveManager.SaveWorldName,
            SaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Player = PlayerControllerData.Create(playerController),

            GroundBuildings = BuildingData.Create(buildings.GerGroundBuildings().ToArray()),
            FloorFrameBuildings = TowerBuildingData.Create(buildings.BuiltFloors.Select(b => b.OwnedTowerBuilding).ToArray()),
            TowerBuildings = TowerBuildingData.Create(buildings.BuiltFloors.SelectMany(b => b.RoomBuildingPlaces).Select(p => p.PlacedBuilding).Where(b => b != null).ToArray()),
            ElevatorCabins = ElevatorCabinData.Create(elevatorCabins.ElevatorCabins),

            CitizenBoatDocks = BoatDockData.Create(boatDocks.CitizenBoatDocks),
            WandererBoatDocks = BoatDockData.Create(boatDocks.WandererDockPoints),
            RaiderBoatDocks = BoatDockData.Create(boatDocks.RaiderDockPoints),
            EvictBoatDocks = BoatDockData.Create(boatDocks.EvictDockPoints),

            Boats = BoatData.Create(boats.Boats),

            Citizens = CitizenData.Create(creatures.Citizens),
            Wanderers = WandererData.Create(creatures.Wanderers),
            Raiders = RaiderData.Create(creatures.Raiders),

            DriftingLoot = DriftingLootSystemData.Create(driftingLoot, driftingLootList),
            CityStorage = InventoryData.Create(cityInventory),

            DailyTasks = DailyTasksData.Create(dailyTasks),
            DailyReward = DailyRewardData.Create(dailyReward),
            Raid = RaidData.Create(raid),
            WanderersSystem = WandererSystemData.Create(wanderers),
            ReviveSystem = ReviveSystemData.Create(revive),
            BuilderEnergy = BuilderEnergyData.Create(constructionEnergy),
            FocusSystem = FocusSystemData.Create(focusManager),
            FoodDrain = FoodDrainData.Create(foodDtain),
            Wind = WindData.Create(wind),
        };
    }
}