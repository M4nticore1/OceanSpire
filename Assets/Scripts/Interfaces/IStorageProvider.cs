using System.Collections.Generic;
using UnityEngine;

public interface IStorageProvider
{
    public IReadOnlyList<ItemStackInstance> StorageStacks { get; }
}