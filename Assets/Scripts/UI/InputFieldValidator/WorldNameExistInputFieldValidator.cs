using UnityEngine;

public class WorldNameExistInputFieldValidator : InputFieldValidator
{
    [SerializeField] private WorldSavesHandler worldSaveHandler;

    protected override bool IsValid(string text)
    {
        var worldsData = worldSaveHandler.AllSavesData;
        if (worldsData == null)
            return false;

        foreach (var data in worldsData) {
            if (data == null)
                continue;

            if (data.WorldName == text)
                return false;
        }

        return true;
    }
}