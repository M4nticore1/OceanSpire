using System;
using UnityEngine;

[Serializable]
public class RandomItemInstance : ItemInstance
{
    public RandomItemInstance(ItemDefinition definition) : base(definition)
    {

    }

    [Header("Random Item")]
    [SerializeField] private int minAmount;
    public int MinAmount => minAmount;

    [SerializeField] private int maxAmount;
    public int MaxAmount => maxAmount;

    public int GetRandomAmount()
    {
        return UnityEngine.Random.Range(MinAmount, maxAmount);
    }
}