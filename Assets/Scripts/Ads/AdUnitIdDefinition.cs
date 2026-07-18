using UnityEngine;

[CreateAssetMenu(fileName = "AdUnitId", menuName = "Ads/AdUnitId")]
public class AdUnitIdDefinition : ScriptableObject
{
    [SerializeField] private string adUnitId = "";
    public string AdUnitId => adUnitId;
}