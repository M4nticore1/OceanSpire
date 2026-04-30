using System.Collections.Generic;
using UnityEngine;

public class RaidComponent : MonoBehaviour
{
    private List<InteractComponent> raiders = new();
    public IReadOnlyList<InteractComponent> Raiders => raiders.AsReadOnly();

    private List<InteractComponent> enteredRaiders = new();
    public IReadOnlyList<InteractComponent> EnteredRaiders => enteredRaiders.AsReadOnly();

    // Raiders
    public void AddRaider(InteractComponent interactor)
    {
        raiders.Add(interactor);
    }

    public void RemoveRaider(InteractComponent interactor)
    {
        raiders.Remove(interactor);
    }

    public void EnterRaider(InteractComponent interactor)
    {
        enteredRaiders.Add(interactor);
    }

    public void ExitRaider(InteractComponent interactor)
    {
        enteredRaiders.Remove(interactor);
    }
}