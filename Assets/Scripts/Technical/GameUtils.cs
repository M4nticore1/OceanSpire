using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
    public static List<GameObject> GetAllChildren(Transform parent)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent)
        {
            children.Add(child.gameObject);
            children.AddRange(GetAllChildren(child));
        }

        return children;
    }
}
