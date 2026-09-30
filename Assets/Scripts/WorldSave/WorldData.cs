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
    public EnergyDrainData EnergyDrain;
    public WindData Wind;

    public static WorldData Default()
    {
        return new WorldData();
    }

    public static WorldData Create(string worldName,
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
        FoodDrainManager foodDrain,
        EnergyDrainManager energyDrain,
        WindManager wind,
        WorldData targetData = null)
    {
        var worldData = targetData != null ? targetData : new WorldData();

        worldData.WorldName = worldName;
        worldData.SaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        worldData.Player = PlayerControllerData.Create(playerController);

        worldData.GroundBuildings = BuildingData.Create(buildings.GetGroundBuildings());
        worldData.FloorFrameBuildings = TowerBuildingData.Create(buildings.BuiltFloors.Select(b => b.OwnedTowerBuilding));
        worldData.TowerBuildings = TowerBuildingData.Create(buildings.BuiltFloors.SelectMany(b => b.RoomBuildingPlaces).Select(p => p.PlacedBuilding).Where(b => b != null));
        worldData.ElevatorCabins = ElevatorCabinData.Create(elevatorCabins.ElevatorCabins);

        worldData.CitizenBoatDocks = BoatDockData.Create(boatDocks.CitizenBoatDocks);
        worldData.WandererBoatDocks = BoatDockData.Create(boatDocks.WandererDockPoints);
        worldData.RaiderBoatDocks = BoatDockData.Create(boatDocks.RaiderDockPoints);
        worldData.EvictBoatDocks = BoatDockData.Create(boatDocks.EvictDockPoints);

        worldData.Boats = BoatData.Create(boats.Boats);

        worldData.Citizens = CitizenData.Create(creatures.Citizens);
        worldData.Wanderers = WandererData.Create(creatures.Wanderers);
        worldData.Raiders = RaiderData.Create(creatures.Raiders);

        worldData.DriftingLoot = DriftingLootSystemData.Create(driftingLoot, driftingLootList);
        worldData.CityStorage = InventoryData.Create(cityInventory);

        worldData.DailyTasks = DailyTasksData.Create(dailyTasks);
        worldData.DailyReward = DailyRewardData.Create(dailyReward);
        worldData.Raid = RaidData.Create(raid);
        worldData.WanderersSystem = WandererSystemData.Create(wanderers);
        worldData.ReviveSystem = ReviveSystemData.Create(revive);
        worldData.BuilderEnergy = BuilderEnergyData.Create(constructionEnergy);
        worldData.FocusSystem = FocusSystemData.Create(focusManager);
        worldData.FoodDrain = FoodDrainData.Create(foodDrain);
        worldData.EnergyDrain = EnergyDrainData.Create(energyDrain);
        worldData.Wind = WindData.Create(wind);

        return worldData;
    }
}