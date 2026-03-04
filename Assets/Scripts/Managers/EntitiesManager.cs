using System.Collections.Generic;
using UnityEngine;

public class EntitiesManager : MonoBehaviour
{
    public List<Human> citizens { get; private set; } = new List<Human>();

    public void Register(Human citizen)
    {
        citizens.Add(citizen);
    }

    public void Unregister(Human citizen)
    {
        citizens.Remove(citizen);
    }
}