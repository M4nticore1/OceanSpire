using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class EquipmentWidget : UIBehaviour
{
    [SerializeField] private EquipmentCategory equipmentCategory;
    public EquipmentCategory EquipmentCategory => equipmentCategory;

    [Header("Equipment Widget")]
    [SerializeField] private TextLocalizer nameText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private Image icon;
    [SerializeField] private CustomButton button;
    protected CustomButton Button => button;

    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private GameObject emptyPanel;

    protected EquipmentComponent equipmentComponent;
    protected EquipmentDefinition equipmentDefinition;

    public static event Action<EquipmentWidget> OnEquipmentWidgetClicked;

    protected override void OnEnable()
    {
        base.OnEnable();

        button.OnReleased.AddListener(OnButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    public virtual void SetEquipmentComponent(EquipmentComponent component)
    {
        equipmentComponent = component;

        UpdateEquipmentPanel(null);
    }

    public virtual void SetEquipmentDefinition(EquipmentDefinition definition)
    {
        equipmentDefinition = definition;

        UpdateNameText(definition);
        UpdatePowerText(definition);
        UpdateIcon(definition);
        UpdateEquipmentPanel(definition);
    }

    private void UpdateNameText(EquipmentDefinition definition)
    {
        if (!definition) return;

        nameText.SetLocalizationItem(definition.NameLocalizationItem);
    }

    private void UpdatePowerText(EquipmentDefinition definition)
    {
        if (!definition) return;

        powerText.SetText(definition.Power.ToString());
    }

    private void UpdateIcon(EquipmentDefinition definition)
    {
        if (!definition) return;

        icon.sprite = definition.ItemIcon;
    }

    private void UpdateEquipmentPanel(EquipmentDefinition definition)
    {
        bool shouldShow = definition && !definition.DefaultEquipment;

        equipmentPanel.SetActive(shouldShow);
        emptyPanel.SetActive(!shouldShow);
    }

    protected void OnButtonClicked()
    {
        OnClicked();
        OnEquipmentWidgetClicked?.Invoke(this);
    }

    protected abstract void OnClicked();
}