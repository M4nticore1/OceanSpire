using System;
using UnityEngine;

public interface IClickable
{
    bool IsClickable { get; }
    void Click();
    void SetClickable(bool value);
    bool ShouldClick();
    event Action OnClicked;
}
