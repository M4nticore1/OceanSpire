using TMPro;
using UnityEngine;

public class BuilderEnergyPanel : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private BuilderEnergyManager builderEnergyManager;
    [SerializeField] private BuilderEnergyMenu builderEnergyMenu;

    [Header("UI")]
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI bonusText;

    private void OnEnable()
    {
        builderEnergyManager.OnEnergyChanged += OnEnergyChanged;
        button.OnReleased.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        builderEnergyManager.OnEnergyChanged -= OnEnergyChanged;
        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    private void UpdateBonusText()
    {
        var bonus = builderEnergyManager.CurrentEnergy;
        bonusText.SetText($"{bonus * 100}%");
    }

    private void OnEnergyChanged(float value)
    {
        UpdateBonusText();
    }

    private void OnButtonClicked()
    {
        builderEnergyMenu.Show();
    }
}