using UnityEngine;

public abstract class ProgressDisplayController : MonoBehaviour
{
    [SerializeField] private ProgressDisplay progressDisplay;
    public ProgressDisplay ProgressDisplay => progressDisplay;

    private ProgressDisplayControllersManager progressDisplayControllersManager => ProgressDisplayControllersManager.Instance;

    protected virtual void Awake()
    {
        ProgressDisplay.Hide();
    }

    private void OnEnable()
    {
        progressDisplayControllersManager.RegisterController(this);
        Subscribe();
    }

    private void OnDisable()
    {
        progressDisplayControllersManager.UnregisterController(this);
        Unsubscribe();
    }

    protected virtual void Start()
    {

    }

    public virtual void Tick()
    {

    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
}