using UnityEngine;

public static class CreatureFactory
{
    public static Human CreateHuman(HumanData data)
    {
        Human prefab = CreaturesList.Instance.GetCreature(data.Id) as Human;

        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {data.Id}");
            return null;
        }

        var human = Object.Instantiate(prefab);
        human.Init(data);

        return human;
    }
}