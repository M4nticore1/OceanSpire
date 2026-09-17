using UnityEngine;

public class ItemStackWidget : AmountableWidget
{
    [Header("Stack Widget")]
    [SerializeField] private ItemStackDefinition stackDefinition;
    public ItemStackDefinition StackDefinition => stackDefinition;

    public ItemStackInstance StackInstance { get; private set; }

    public virtual void SetStackInstance(ItemStackInstance stackInstance)
    {
        if (stackInstance == null) {
            Debug.LogError($"[{nameof(ItemWidget)}] Stack Instance is not valid!");
            return;
        }

        if (stackInstance == StackInstance)
            return;

        StackInstance = stackInstance;

        SetStackDefinition(stackInstance.Definition);
    }

    public virtual void SetStackDefinition(ItemStackDefinition stackDefinition)
    {
        if (stackDefinition == null) {
            Debug.LogError($"[{nameof(ItemWidget)}] Stack Definition is not valid!");
            return;
        }

        if (stackDefinition == this.stackDefinition)
            return;

        this.stackDefinition = stackDefinition;

        UpdateName();
        UpdateIcon();
    }

    protected override void HandleInfoButtonClicked()
    {
        var informationMenu = InformationMenu.Instance;

        if (informationMenu == null)
            return;

        informationMenu.Show(StackInstance);
    }

    protected override LocalizationItem GetName()
    {
        if (stackDefinition == null) return null;

        return stackDefinition.NameLocalizationItem;
    }

    protected override Sprite GetIcon()
    {
        if (stackDefinition == null) return null;

        return stackDefinition.Icon;
    }

    protected override bool ShouldDestroy()
    {
        if (!base.ShouldDestroy()) return false;

        return false;
    }
}