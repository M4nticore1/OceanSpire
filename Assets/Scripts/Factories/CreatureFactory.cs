using UnityEngine;

public static class CreatureFactory
{
    public static Human CreateHuman(Creature prefab, HumanData data)
    {
        var humanPrefab = prefab as Human;
        if (!humanPrefab) {
            Debug.LogError("Selected prefab is not Human");
            return null;
        }

        var human = Object.Instantiate(humanPrefab, data.Position.Vector3(), Quaternion.Euler(data.Rotation.Vector3()));
        human.Init(data);

        return human;
    }
}