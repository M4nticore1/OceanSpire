using System;
using UnityEngine;

public interface IItemAmount
{
    public int Amount { get; }
    public event Action OnAmountChanged;
}
