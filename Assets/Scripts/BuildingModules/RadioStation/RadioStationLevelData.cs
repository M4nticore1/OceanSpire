using UnityEngine;

[CreateAssetMenu(fileName = "RadioStationLevelData", menuName = "Scriptable Objects/RadioStationLevelData")]
public class RadioStationLevelData : BuildingModuleLevelData
{
    [SerializeField] private float wandererCooldownSpeedBonus = 0.1f;
    public float WandererCooldownSpeedBonus => wandererCooldownSpeedBonus;
}
