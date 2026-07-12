using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
    public static List<GameObject> GetAllChildren(Transform parent, Predicate<GameObject> shouldInclude = null)
    {
        var children = new List<GameObject>();

        foreach (Transform child in parent) {
            var childGO = child.gameObject;
            if (shouldInclude != null && !shouldInclude(childGO)) continue;

            children.Add(childGO);
            children.AddRange(GetAllChildren(child, shouldInclude));
        }

        return children;
    }
}