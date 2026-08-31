using UnityEngine;

public abstract class InteractionToggler : MonoBehaviour
{
    protected virtual void Awake()
    {

    }

    public abstract void EnableInteraction();
    public abstract void DisableInteraction();
}