using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickableInteractionToggler : InteractionToggler
{
    private Dictionary<IClickable, bool> clickablesDict = new();

    [SerializeField] private string[] excludedClassNames;
    private List<Type> excludedClasses = new();

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < excludedClassNames.Length; i++) {
            var className = excludedClassNames[i];
            if (string.IsNullOrEmpty(className)) continue;

            var resolvedType = Type.GetType(className);
            if (resolvedType == null) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    resolvedType = assembly.GetType(className);
                    if (resolvedType != null) break;
                }
            }

            if (resolvedType == null) continue;
            excludedClasses.Add(resolvedType);
        }
    }

    public override void EnableInteraction()
    {
        foreach (var select in clickablesDict.Keys) {
            if (select == null) continue; 

            select.IsClickable = clickablesDict[select];
        }

        clickablesDict.Clear();
    }

    public override void DisableInteraction()
    {
        var monobehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var monobehaviour in monobehaviours) {
            if (monobehaviour == null) continue;

            var clickable = monobehaviour.GetComponent<IClickable>();
            if (clickable == null) continue;

            bool isExcluded = false;
            foreach (var excludedClass in excludedClasses) {
                if (excludedClass == null) continue;

                if (monobehaviour.GetComponent(excludedClass) != null) {
                    isExcluded = true;
                    break;
                }
            }

            if (isExcluded) continue;
            if (!clickablesDict.TryAdd(clickable, clickable.IsClickable)) continue;

            clickable.IsClickable = false;
        }
    }
}