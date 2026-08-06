//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class RaidComponent : MonoBehaviour
//{
//    [field: SerializeField] private List<Raider> raiders = new();
//    public IReadOnlyList<Raider> Raiders => raiders.AsReadOnly();

//    [field: SerializeField] private List<Raider> currentRaiders = new();
//    public IReadOnlyList<Raider> CurrentRaiders => currentRaiders.AsReadOnly();

//    public event Action<Raider> OnRaiderAdded;
//    public event Action<Raider> OnRaiderRemoved;

//    public event Action<Raider> OnCurrentRaiderAdded;
//    public event Action<Raider> OnCurrentRaiderRemoved;

//    // Raiders
//    public void AddRaider(Raider raider)
//    {
//        if (raiders.Contains(raider)) return;

//        raiders.Add(raider);
//        OnRaiderAdded?.Invoke(raider);
//    }

//    public void RemoveRaider(Raider raider)
//    {
//        if (!raiders.Contains(raider)) return;

//        raiders.Remove(raider);
//        OnRaiderRemoved?.Invoke(raider);
//    }

//    public void AddCurrentRaider(Raider raider)
//    {
//        if (currentRaiders.Contains(raider)) return;

//        currentRaiders.Add(raider);
//        OnCurrentRaiderAdded?.Invoke(raider);
//    }

//    public void RemoveCurrentRaider(Raider raider)
//    {
//        if (!currentRaiders.Contains(raider)) return;

//        currentRaiders.Remove(raider);
//        OnCurrentRaiderRemoved?.Invoke(raider);
//    }
//}