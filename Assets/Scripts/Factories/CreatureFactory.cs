using UnityEngine;

public static class CreatureFactory
{
    public static Creature CreateCreature(int id, CreatureEntry data)
    {
        Creature prefab = CreaturesList.Instance.Creatures[id];
        if (!prefab) {
            Debug.LogError($"No prefab found for Creature ID {id}");
            return null;
        }

        var obj = Object.Instantiate(prefab);
        obj.Init(data);

        Human human = obj as Human;
        if (human)
            EventBus.InvokeCitizenAdded(human);

        return obj;
    }
}
