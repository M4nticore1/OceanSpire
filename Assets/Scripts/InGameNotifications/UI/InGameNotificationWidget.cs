using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameNotificationWidget : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CustomButton descriptionButton;
    public CustomButton DescriptionButton => descriptionButton;

    [SerializeField] private CustomButton hideButton;
    public CustomButton HideButton => hideButton;

    [SerializeField] private TextLocalizer nameText;
    public TextLocalizer NameText => nameText;

    [SerializeField] private TextLocalizer descriptionText;
    public TextLocalizer DescriptionText => descriptionText;

    [SerializeField] private Image iconImage;
    public Image IconImage => iconImage;

    [SerializeField] private AnimatedPanel descriptionPanel;
    public AnimatedPanel DescriptionPanel => descriptionPanel;

    [SerializeField] private FitSizeToContent fitSize;

    public InGameNotificationData NotificationData { get; private set; }

    private InGameNotificationSeverityDefinition severityDefinition;
    private int priority = 50;

    public static event Action<InGameNotificationWidget> OnNotificationHidden;

    private void OnEnable()
    {
        if (descriptionButton != null) {
            descriptionButton.OnSelected.AddListener(HandleDescriptionButtonSelected);
            descriptionButton.OnDeselected.AddListener(HandleDescriptionButtonDeselected);
        }

        if (hideButton != null) {
            hideButton.OnReleased.AddListener(HandleHideButtonClicked);
        }
    }

    private void OnDisable()
    {
        if (descriptionButton != null) {
            descriptionButton.OnSelected.RemoveListener(HandleDescriptionButtonSelected);
            descriptionButton.OnDeselected.RemoveListener(HandleDescriptionButtonDeselected);
        }

        if (hideButton != null) {
            hideButton.OnReleased.RemoveListener(HandleHideButtonClicked);
        }
    }

    private void Start()
    {
        if (descriptionButton == null) {
            Debug.LogError($"[{nameof(InGameNotificationWidget)}] Description Button is not valid at {this}!");
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
        NotificationData = notificationData;

        nameText.SetLocalizationItem(notificationData.NameLocalization);
        nameText.SetPlaceHolderLocalization(notificationData.NameLocalizationHolder);

        descriptionText.SetLocalizationItem(notificationData.DescriptionLocaliztion);
        descriptionText.SetPlaceHolderLocalization(notificationData.DescriptionLocalizationHolder);

        iconImage.sprite = notificationData.Icon;
        severityDefinition = notificationData.SeverityDefinition;
        priority = notificationData.Priority;

        UpdateSiblingIndex();
        UpdateButtonColor();
        UpdateHideButtonActive(severityDefinition != null ? severityDefinition.NotificationType : InGameNotificationType.Message);

        if (descriptionPanel != null) {
            descriptionPanel.SetAnimationProgress(0f);
        }

        fitSize.UpdateSize();
    }

    public void Hide()
    {
        Destroy(gameObject);
        OnNotificationHidden?.Invoke(this);
    }

    public void SetButtonSelectGroup(SelectGroup selectGroup)
    {
        if (descriptionButton != null) {
            descriptionButton.SetSelectGroup(selectGroup);
        }
    }

    private void UpdateSiblingIndex()
    {
        var parent = transform.parent;
        if (parent == null) return;

        int targetIndex = 0;

        for (int i = 0; i < parent.childCount; i++) {
            var child = parent.GetChild(i);
            if (child == transform) continue;

            if (child.TryGetComponent<InGameNotificationWidget>(out var otherWidget)) {
                if (otherWidget.NotificationData.Priority <= priority) {
                    targetIndex = i + 1;
                }
                else {
                    break;
                }
            }
        }

        transform.SetSiblingIndex(targetIndex);
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

    private void UpdateHideButtonActive(InGameNotificationType notificationType)
    {
        if (hideButton == null) return;

        hideButton.gameObject.SetActive(notificationType == InGameNotificationType.Message);
    }

    private void HandleDescriptionButtonSelected()
    {
        descriptionPanel.Show();
    }

    private void HandleDescriptionButtonDeselected()
    {
        descriptionPanel.Hide();
    }

    private void HandleHideButtonClicked()
    {
        Hide();
    }
}