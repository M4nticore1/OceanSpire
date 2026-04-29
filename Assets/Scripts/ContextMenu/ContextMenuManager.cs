using System;
using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance { get; private set; }

    public event Action<ContextMenuTarget> onContextMenuTargetSelected;

    private void Awake()
    {
        if (Instance != null) {
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        ContextMenuTarget.onTargetSelected += OnContextTargetSelected;
    }

    private void OnDisable()
    {
        ContextMenuTarget.onTargetSelected -= OnContextTargetSelected;
    }

    private void OnContextTargetSelected(ContextMenuTarget target)
    {
        onContextMenuTargetSelected?.Invoke(target);
    }
}