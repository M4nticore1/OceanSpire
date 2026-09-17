using UnityEngine;

public class DiscardInfoPanelWidget : InfoPanelWidget
{
    [Header("Main")]
    [SerializeField] private DiscardItemMenu discardItemMenu;

    [Header("UI")]
    [SerializeField] private CustomButton discardButton;

    private DiscardInfoPanelData discardInfoPanelData => InfoPanelData as DiscardInfoPanelData;

    protected override void OnEnable()
    {
        base.OnEnable();

        discardButton.OnReleased.AddListener(HandleDiscardButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        discardButton.OnReleased.RemoveListener(HandleDiscardButtonClicked);
    }

    private void HandleDiscardButtonClicked()
    {
        if (discardInfoPanelData == null) {
            Debug.LogError($"[{nameof(DiscardInfoPanelWidget)}] Discard Info Panel Data is not valid!");
            return;
        }

        discardItemMenu.Show(discardInfoPanelData.Item);
    }
}