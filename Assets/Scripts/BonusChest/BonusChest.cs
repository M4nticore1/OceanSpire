using UnityEngine;

public class BonusChest : MonoBehaviour, IClickable
{
    [SerializeField] private BonusChestMenu bonusChestMenu;

    public void Click()
    {
        bonusChestMenu.Open();
    }

    public bool ShouldClick()
    {
        return true;
    }
}