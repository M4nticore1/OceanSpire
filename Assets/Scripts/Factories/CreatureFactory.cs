using UnityEngine;

public static class CreatureFactory
{
    public static Human CreateHuman(Creature prefab, Vector3 position, Quaternion rotation, HumanData data)
    {
        var humanPrefab = prefab as Human;
        if (!humanPrefab) {
            Debug.Log("Selected prefab is not Human");
            return null;
        }

        var human = Object.Instantiate(humanPrefab, position, rotation);
        human.Init(data);

        return human;
    }
}