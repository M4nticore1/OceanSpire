using UnityEngine;

public static class ContextMenuFactory
{
    public static T CreateContextMenu<T, TData>(T menuToSpawn, TData data, Transform parent) where T : ContextMenu<TData> where TData : class
    {
        T menu = Object.Instantiate(menuToSpawn, parent);
        menu.Init(data);
        return menu;
    }
}