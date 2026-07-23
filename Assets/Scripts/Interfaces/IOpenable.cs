using System;
using UnityEngine;

public interface IOpenable
{
    public bool IsShowed { get; }

    public void Show();
    public void Hide();

    public event Action OnShowed;
    public event Action OnHidden;
}