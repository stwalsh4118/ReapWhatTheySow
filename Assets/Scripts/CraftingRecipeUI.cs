using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeUI : MonoBehaviour
{

    public List<CraftingRecipe> craftingRecipes;
    public List<GameObject> RecipeEntries;
    public GameObject RecipeHolder;

    private void Start() {
        UpdateCraftingUI(craftingRecipes);
    }   

    public void UpdateCraftingUI(List<CraftingRecipe> craftingRecipes) {
        for(int i = 0; i < craftingRecipes.Count; i++) {
            CraftingRecipe craftingRecipe = craftingRecipes[i];
            GameObject recipe = Instantiate(Resources.Load("Prefabs/UI/RecipeEntry", typeof(GameObject))) as GameObject;
            recipe.transform.Find("RecipeSlot/Item").GetComponent<Image>().sprite = craftingRecipes[i].itemProduced.icon;
            recipe.transform.Find("RecipeSlot/Amount").GetComponent<TextMeshProUGUI>().text = craftingRecipes[i].amountProduced.ToString();
            recipe.transform.Find("RecipeSlot").GetComponent<Button>().onClick.AddListener(() => {CraftItem(craftingRecipe);});
            recipe.transform.SetParent(RecipeHolder.transform, false);
            Transform recipeComponentHolder = recipe.transform.Find("Scroll View/Viewport/Content");

            for(int j = 0; j < craftingRecipes[i].components.Length; j++) {
                GameObject recipeComponent = Instantiate(Resources.Load("Prefabs/UI/RecipeComponentSlot", typeof(GameObject))) as GameObject;
                recipeComponent.transform.SetParent(recipeComponentHolder, false);
                recipeComponent.transform.Find("Item").GetComponent<Image>().sprite = craftingRecipes[i].components[j].item.icon;
                recipeComponent.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = craftingRecipes[i].components[j].amount.ToString();

            }
        }
    }

    public void CraftItem(CraftingRecipe craftingRecipe) {
        ItemProductionManager.instance.ProduceItemFromRecipe(craftingRecipe);
    }
}
