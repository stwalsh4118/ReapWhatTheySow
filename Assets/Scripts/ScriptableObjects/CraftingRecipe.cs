using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "new CraftingRecipe")]
public class CraftingRecipe : ScriptableObject
{
    public RecipeComponent[] components;
    public Item itemProduced;
}

[System.Serializable]
public class RecipeComponent {
    public Item item;
    public int amount;
}
