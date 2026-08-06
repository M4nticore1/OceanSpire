using System.Collections.Generic;
using UnityEngine;

public interface IRaidable
{
    public List<ItemInstance> GetRaidLoot();
}