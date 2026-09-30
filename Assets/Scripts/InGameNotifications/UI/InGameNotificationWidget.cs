using UnityEngine;
using UnityEngine.UI;

public enum InGameNotificationId
{
    Starvation,
    EnergyShortage
}

public class InGameNotificationWidget : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private InGameNotificationId notificationId;
    public InGameNotificationId NotificationId => notificationId;

    [SerializeField] private InGameNotificationSeverityDefinition severityDefinition;
    public InGameNotificationSeverityDefinition SeverityDefinition => severityDefinition;

    [SerializeField] private int priority = 50;
    public int Priorit => priority;

    [Header("UI")]
    [SerializeField] private CustomButton descriptionButton;
    public CustomButton DescriptionButton => descriptionButton;

    [SerializeField] private Image descriptionBackground;
    public Image DescriptionBackground => descriptionBackground;

    [SerializeField] private AnimatedPanel descriptionPanel;
    public AnimatedPanel DescriptionPanel => descriptionPanel;

    private void Awake()
    {
        if (severityDefinition == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Severity Definition is not valid at {this}!");
        }
        if (descriptionButton == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Button is not valid at {this}!");
        }
        if (descriptionBackground == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Background is not valid at {this}!");
        }
        if (descriptionPanel == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Panel is not valid at {this}!");
        }

        UpdateSiblingIndex();
        UpdateButtonColor();
        UpdateDescriptionBackgroundColor();

        if (descriptionPanel != null) {
            descriptionPanel.SetAnimationProgress(0f);
        }
    }

    private void OnEnable()
    {
        if (descriptionButton != null) {
            descriptionButton.OnSelected.AddListener(HandleDescriptionButtonSelected);
            descriptionButton.OnDeselected.AddListener(HandleDescriptionButtonDeselected);
        }
    }

    private void OnDisable()
    {
        if (descriptionButton != null) {
            descriptionButton.OnSelected.RemoveListener(HandleDescriptionButtonSelected);
            descriptionButton.OnDeselected.RemoveListener(HandleDescriptionButtonDeselected);
        }
    }

    public void SetButtonSelectGroup(SelectGroup selectGroup)
    {
        if (descriptionButton != null) {
            descriptionButton.SetSelectGroup(selectGroup);
        }
    }

    private void UpdateSiblingIndex()
    {
        transform.SetSiblingIndex(priority);
    }

    private void UpdateButtonColor()
    {
        if (descriptionButton == null) return;
        if (severityDefinition == null) return;

        descriptionButton.idleState.bodyColor = severityDefinition.NormalColor;
        descriptionButton.hoveredState.bodyColor = severityDefinition.HoveredColor;
        descriptionButton.pressedState.bodyColor = severityDefinition.PressedColor;
        descriptionButton.selectedState.bodyColor = severityDefinition.SelectedColor;
    }

    private void UpdateDescriptionBackgroundColor()
    {
        if (descriptionBackground == null) return;
        if (severityDefinition == null) return;

        descriptionBackground.color = severityDefinition.SelectedColor;
    }

    private void HandleDescriptionButtonSelected()
    {
        descriptionPanel.Show();
    }

    private void HandleDescriptionButtonDeselected()
    {
        descriptionPanel.Hide();
    }
}