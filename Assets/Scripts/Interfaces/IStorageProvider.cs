using System.Collections.Generic;
using UnityEngine;

public interface IStorageProvider
{
    public IReadOnlyList<ItemStack> StorageStacks { get; }
}