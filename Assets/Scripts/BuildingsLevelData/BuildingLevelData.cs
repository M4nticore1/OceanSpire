using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceToBuild
{
    public ItemDefinition itemData;
    public int amount;
}

public abstract class BuildingLevelData : ScriptableObject, ILocalizable
{
    [Header("Building")]
    [SerializeField] private ItemInstance[] resourcesToBuild;
    public ItemInstance[] ResourcesToBuild => resourcesToBuild;

    [SerializeField] private int maxResidentsCount = 0;
    public int MaxHumansCount => maxResidentsCount;

    [SerializeField] private int constructionTime = 0;
    public int UpgradeTime => constructionTime;

    [SerializeField] private Sprite buildingThumb;
    public Sprite BuildingThumb => buildingThumb;

    public Dictionary<string, string> GetLocalization()
    {
        var buildTime = "";
        var speedBonus = BuilderEnergyManager.Instance.CurrentEnergy;
        var speedBonusText = (speedBonus * 100).ToString("F0");
        var timeWithBonus = (int)(constructionTime * (1f - speedBonus));

        var constructionTimeText = TimeFormatter.SecondsToTimer(timeWithBonus);
        var bonusText = $"(-{speedBonusText}%)";
        buildTime = speedBonus > 0 ? $"<color=green>{constructionTimeText} {bonusText}</color>" : constructionTimeText;

        return new Dictionary<string, string>()
        {
            { "buildTime", buildTime.ToString() }
        };
    }
}
