using UnityEngine;

public class ItemInformationMenu : InformationMenu
{
    public static ItemInformationMenu Instance { get; private set; }

    private ItemInstance item;

    [Header("Item Information")]
    [SerializeField] private DiscardItemMenu discardItemMenu;
    [SerializeField] private CustomButton discardItemButton;

    protected override void Awake()
    {
        base.Awake();

        if (Instance) {
            Debug.LogError($"[{nameof(ItemInformationMenu)}] There is another Item Information Menu in the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        discardItemButton.OnReleased.AddListener(OnDiscardButtonClicked);
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        discardItemButton.OnReleased.RemoveListener(OnDiscardButtonClicked);
    }

    public void Show(ItemInstance itemInstance)
    {
        if (itemInstance == null) {
            Debug.LogError($"[{nameof(ItemInformationMenu)}] Item Instance is not valid!");
            return;
        }

        item = itemInstance;
        Show(itemInstance as IInformationable);
    }

    private void OnDiscardButtonClicked()
    {
        if (item == null) {
            Debug.LogError($"[{nameof(ItemInformationMenu)}] Item is not valid!");
            return;
        }

        discardItemMenu.Show(item);
    }
}