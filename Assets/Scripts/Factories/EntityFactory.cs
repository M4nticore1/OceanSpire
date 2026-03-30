using UnityEngine;

public static class EntityFactory
{
    public static Human CreateHuman(HumanEntry data)
    {
        Human prefab = CreaturesList.Instance.Creatures[data.id] as Human;
        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {data.id}");
            return null;
        }

        var human = Object.Instantiate(prefab);
        human.Init(data);
        return human;
    }
}