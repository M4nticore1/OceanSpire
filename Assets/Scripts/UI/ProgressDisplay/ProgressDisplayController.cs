using UnityEngine;

public abstract class ProgressDisplayController : MonoBehaviour
{
    [SerializeField] private ProgressDisplay progressDisplay;
    public ProgressDisplay ProgressDisplay => progressDisplay;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
}