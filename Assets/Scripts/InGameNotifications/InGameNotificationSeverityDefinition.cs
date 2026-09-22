using UnityEngine;

public enum InGameNotificationType
{
    Message,
    Warning,
    Alert
}

[CreateAssetMenu(fileName = "InGameNotificationSeverityDefinition", menuName = "Scriptable Objects/InGameNotificationSeverityDefinition")]
public class InGameNotificationSeverityDefinition : ScriptableObject
{
    [SerializeField] private InGameNotificationType notificationType;
    public InGameNotificationType NotificationType => notificationType;

    [SerializeField] private Color normalColor = Color.clear;
    public Color NormalColor => normalColor;

    [SerializeField] private Color hoveredColor = Color.clear;
    public Color HoveredColor => hoveredColor;

    [SerializeField] private Color pressedColor = Color.clear;
    public Color PressedColor => pressedColor;

    [SerializeField] private Color selectedColor = Color.clear;
    public Color SelectedColor => selectedColor;
}