using UnityEngine;

public abstract class CustomUI : MonoBehaviour
{
    protected CustomUIManager customUIManager => CustomUIManager.Instance;

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        
    }

    protected virtual void Start()
    {
        
    }

    public virtual void Tick()
    {

    }
}