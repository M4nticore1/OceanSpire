using System;
using UnityEngine;

public interface IOpenable
{
    public void Show();
    public void Hide();

    public event Action OnShown;
    public event Action OnHidden;
}