using UnityEngine;

public class WorldNameExistInputFieldValidator : InputFieldValidator
{
    [SerializeField] private WorldSaveHandler worldSaveHandler;

    protected override bool IsValid(string text)
    {
        var worldsData = worldSaveHandler.AllSaveData;
        if (worldsData == null) return false;

        foreach (var data in worldsData) {
            if (data == null) continue;
            if (data.WorldName != text) continue;

            return false;
        }

        return true;
    }
}