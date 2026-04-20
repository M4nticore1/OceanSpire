using UnityEngine;

[CreateAssetMenu(fileName = "DailyTaskDefinition", menuName = "Scriptable Objects/DailyTaskDefinition")]
public class DailyTaskDefinition : ScriptableObject
{
    [SerializeField] private ItemInstance reward;
    public ItemInstance Reward => reward;


}
