using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProductionManager : MonoBehaviour
{
    public static ItemProductionManager instance;
    private void Awake() {
        if(instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            instance = this;
        }
    }

    public bool ProduceItemFromRecipe(CraftingRecipe recipe) {
        for(int i = 0; i < recipe.components.Length; i++) {
            bool componentInInventory = Inventory.instance.ItemInInventory(recipe.components[i].item);

            if(!componentInInventory) {
                Debug.Log("Recipe Component " + recipe.components[i].item.name + " is not in your inventory");
                return false;
            }

            List<int> indicesOfItemInInventory = Inventory.instance.GetInventoryIndices(recipe.components[i].item);

            int sumOfItemInInventory = 0;
            for(int index = 0; index < indicesOfItemInInventory.Count; index++) {
                sumOfItemInInventory += Inventory.instance.StackSizes[indicesOfItemInInventory[index]];
            }

            if(sumOfItemInInventory < recipe.components[i].amount) {
                Debug.Log("Not enough " + recipe.components[i].item.name + " in your inventory");
                return false;
            }
        }

        for(int i = 0; i < recipe.components.Length; i++) {
            Inventory.instance.RemoveItem(recipe.components[i].item, 0, recipe.components[i].amount);
        }

        Inventory.instance.AddItem(recipe.itemProduced, recipe.amountProduced);
        Inventory.instance.UpdateInventory(Inventory.instance.Items);
        Debug.Log("Added " + recipe.itemProduced.name + " to your inventory");

        return true;
    }
}
