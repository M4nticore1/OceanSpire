using UnityEngine;

public abstract class SettingWidget : MonoBehaviour
{
    protected PlayerSettingsManager playerSettingsManager => PlayerSettingsManager.Instance;
}