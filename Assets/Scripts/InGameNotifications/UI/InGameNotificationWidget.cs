using UnityEngine;
using UnityEngine.UI;

public class InGameNotificationWidget : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CustomButton descriptionButton;
    public CustomButton DescriptionButton => descriptionButton;

    [SerializeField] private TextLocalizer nameText;
    public TextLocalizer NameText => nameText;

    [SerializeField] private TextLocalizer descriptionText;
    public TextLocalizer DescriptionText => descriptionText;

    [SerializeField] private Image descriptionBackground;
    public Image DescriptionBackground => descriptionBackground;

    [SerializeField] private AnimatedPanel descriptionPanel;
    public AnimatedPanel DescriptionPanel => descriptionPanel;

    private InGameNotificationSeverityDefinition severityDefinition;
    private int priority = 50;

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

    private void Start()
    {
        if (descriptionButton == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Button is not valid at {this}!");
        }
        if (descriptionBackground == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Background is not valid at {this}!");
        }
        if (descriptionPanel == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Panel is not valid at {this}!");
        }
        if (severityDefinition == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Severity Definition is not valid at {this}!");
        }
    }

    public void Init(InGameNotificationData notificationData)
    {
        if (notificationData == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] NotificationData is not valid!");
            return;
        }

        nameText.SetLocalizationItem(notificationData.NameLocalization);
        nameText.SetPlaceHolderLocalization(notificationData.NameLocalizationHolder);

        descriptionText.SetLocalizationItem(notificationData.DescriptionLocaliztion);
        descriptionText.SetPlaceHolderLocalization(notificationData.DescriptionLocalizationHolder);

        severityDefinition = notificationData.SeverityDefinition;
        priority = notificationData.Priority;

        UpdateSiblingIndex();
        UpdateButtonColor();
        UpdateDescriptionBackgroundColor();

        if (descriptionPanel != null) {
            descriptionPanel.SetAnimationProgress(0f);
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
        if (descriptionButton == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Button is not valid at {this}!");
            return;
        }

        if (severityDefinition == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Severity Definition is not valid at {this}!");
            return;
        }

        descriptionButton.idleState.bodyColor = severityDefinition.NormalColor;
        descriptionButton.hoveredState.bodyColor = severityDefinition.HoveredColor;
        descriptionButton.pressedState.bodyColor = severityDefinition.PressedColor;
        descriptionButton.selectedState.bodyColor = severityDefinition.SelectedColor;

        descriptionButton.EndTransitionAnimation();
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