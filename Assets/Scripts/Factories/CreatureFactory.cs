using UnityEngine;

public static class CreatureFactory
{
    public static Human CreateCitizen(HumanDataV1 data)
    {
        Human prefab = CreaturesList.Instance.GetCitizen(data.id) as Human;

        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {data.id}");
            return null;
        }

        var human = Object.Instantiate(prefab);
        human.Init(data);

        return human;
    }

    public static Human CreateWanderer(HumanDataV1 data)
    {
        Human prefab = CreaturesList.Instance.GetWanderer(data.id) as Human;

        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {data.id}");
            return null;
        }

        var human = Object.Instantiate(prefab);
        human.Init(data);

        return human;
    }

    public static Human CreateRaider(HumanDataV1 data)
    {
        Human prefab = CreaturesList.Instance.GetRaider(data.id) as Human;

        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {data.id}");
            return null;
        }

        var human = Object.Instantiate(prefab);
        human.Init(data);

        return human;
    }
}