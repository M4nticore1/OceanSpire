using System.Collections.Generic;
using UnityEngine;

public interface IRecipeProvider
{
    public CraftItemDefinition[] Crafts { get; }
}