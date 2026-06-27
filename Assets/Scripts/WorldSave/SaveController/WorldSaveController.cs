using UnityEngine;

public abstract class WorldSaveController : MonoBehaviour
{
    [SerializeField] private WorldSaveManager worldSaveManager;

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    protected virtual void OnSubscribe()
    {

    }

    protected virtual void OnUnsubscribe()
    {

    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;

        return true;
    }

    protected void SaveWorld()
    {
        worldSaveManager.SaveWorld();
    }

    private void TrySubscribe()
    {
        if (!ShouldSubscribe()) return;

        Subscribe();
    }

    private void TryUnsubscribe()
    {
        if (!ShouldUnsubscribe()) return;

        Unsubscribe();
    }

    private void Subscribe()
    {
        OnSubscribe();
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        OnUnsubscribe();
        isSubscribed = false;
    }
}