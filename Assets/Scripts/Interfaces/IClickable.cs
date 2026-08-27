using System;
using UnityEngine;

public interface IClickable
{
    bool IsClickable { get; set; }
    void Click();
    bool ShouldClick();
    event Action OnClicked;
}
