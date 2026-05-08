using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentWidget : UIBehaviour
{
    [SerializeField] private bool equipWidget = false;
    [SerializeField] private EquipmentCategory equipmentCategory;

    [Header("Menus")]
    [SerializeField] private SelectEquipmentMenu selectEquipmentMenu;

    [Header("Panels")]
    [SerializeField] private GameObject selectEquipmentPanel;
    [SerializeField] private GameObject equipmentPanel;

    [Header("EquipmentPanel")]
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image icon;
    [SerializeField] private CustomButton button;

    private EquipmentDefinition equipment;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.onReleased.AddListener(OnButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.onReleased.RemoveListener(OnButtonClicked);
    }

    public void SetEquipment(EquipmentDefinition definition)
    {
        bool shouldShow = definition && !definition.DefaultEquipment;

        selectEquipmentPanel.SetActive(!shouldShow);
        equipmentPanel.SetActive(shouldShow);

        if (shouldShow) return;

        nameText.SetLocalizationItem(definition.NameLocalization);
        powerText.SetText(definition.Power.ToString());
        icon.sprite = definition.ItemIcon;
    }

    public void SetEquipWidget(bool value)
    {
        equipWidget = value;
    }

    private void OnButtonClicked()
    {
        if (equipWidget) {
            var human = SelectManager.Instance.GetSelectedHuman();
            if (!human) return;

            equipment.Equip(human);
        }
        else {
            selectEquipmentMenu.Open(equipmentCategory);
        }
    }
}