using UnityEngine;

public class CitizensManager : MonoBehaviour
{
    [SerializeField] private CreaturesManager creaturesManager;

    public int GetAvaliableCitizensCount()
    {
        var count = 0;
        foreach (var citizen in creaturesManager.Citizens) {
            if (!citizen) continue;
            if (!citizen.IsCitizenAvaliable()) continue;

            count++;
        }

        return count;
    }
}