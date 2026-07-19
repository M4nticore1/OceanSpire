using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
    public FocusSystemData FocusSystem;
    public InventoryData CityStorage;
    public DailyTasksData DailyTasks;
    public DailyRewardData DailyReward;
    public RaidData Raid;
    public WandererSystemData WanderersSystem;
    public BuilderEnergyData BuilderEnergy;
    public ReviveSystemData ReviveSystem;
    public WindData Wind;

    public static WorldData Create(WorldSaveHandler saveManager,
        PlayerController playerController,
        BuildingsManager buildings,
        ElevatorCabinsManager elevatorCabins,
        DockPointsManager boatDocks,
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
        WindManager wind)
    {
        return new WorldData() {
            WorldName = saveManager.SaveWorldName,
            SaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Player = PlayerControllerData.Create(playerController),

            GroundBuildings = BuildingData.Create(buildings.GerGroundBuildings().ToArray()),
            FloorFrameBuildings = TowerBuildingData.Create(buildings.BuiltFloors.Select(b => b.OwnedTowerBuilding).ToArray()),
            TowerBuildings = TowerBuildingData.Create(buildings.BuiltFloors.SelectMany(b => b.RoomBuildingPlaces).Select(p => p.PlacedBuilding).Where(b => b != null).ToArray()),
            ElevatorCabins = ElevatorCabinData.Create(elevatorCabins.ElevatorCabins.ToArray()),

            CitizenBoatDocks = BoatDockData.Create(boatDocks.CitizenBoatDocks.ToArray()),
            WandererBoatDocks = BoatDockData.Create(boatDocks.WandererDockPoints.ToArray()),
            RaiderBoatDocks = BoatDockData.Create(boatDocks.RaiderDockPoints.ToArray()),
            EvictBoatDocks = BoatDockData.Create(boatDocks.EvictDockPoints.ToArray()),

            Boats = BoatData.Create(boats.BoatsDict.Values.ToArray()),

            Citizens = CitizenData.Create(creatures.Citizens.ToArray()),
            Wanderers = WandererData.Create(creatures.Wanderers.ToArray()),
            Raiders = RaiderData.Create(creatures.Raiders.ToArray()),

            DriftingLoot = DriftingLootSystemData.Create(driftingLoot, driftingLootList),
            CityStorage = InventoryData.Create(cityInventory),

            DailyTasks = DailyTasksData.Create(dailyTasks),
            DailyReward = DailyRewardData.Create(dailyReward),
            Raid = RaidData.Create(raid),
            WanderersSystem = WandererSystemData.Create(wanderers),
            ReviveSystem = ReviveSystemData.Create(revive),
            BuilderEnergy = BuilderEnergyData.Create(constructionEnergy),
            FocusSystem = FocusSystemData.Create(focusManager),
            Wind = WindData.Create(wind),
        };
    }
}