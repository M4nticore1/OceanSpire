using System;
using UnityEngine;

public interface IAmountable
{
    public int Amount { get; }
    public event Action<int> OnAmountChanged;
}
