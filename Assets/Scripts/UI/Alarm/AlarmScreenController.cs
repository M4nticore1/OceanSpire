using UnityEngine;

public abstract class AlarmScreenController : MonoBehaviour
{
    [SerializeField] private FlickingImage alarmBackground;
    [SerializeField] private int priority;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TrySubscribe();
    }

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (!isSubscribed) return false;

        return true;
    }

    protected void DisplayAlarmScreen()
    {

    }

    protected void HideAlarmScreen()
    {

    }
}