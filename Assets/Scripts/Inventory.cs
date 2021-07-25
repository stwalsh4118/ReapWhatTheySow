using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Inventory : MonoBehaviour
{

    public List<GameObject> HotbarSlots;
    public List<GameObject> InventorySlots;
    public GameObject InventoryMenu;
    public GameObject InventoryUI;
    public GameObject InventorySlot;
    public GameObject Crosshair;
    public List<Item> Items;
    public List<int> StackSizes;
    int maxStackSize = 2;
    public int numInventorySlots = 35;
    public int numHotbarSlots = 9;
    public int activeHotbarSlot = 1;
    public int mostRecentDragStart;
    Color hotbarSlotHighlight = new Color();
    public static Inventory instance;


    void Start()
    {
        //set the static instance
        instance = this;

        InventoryMenu.SetActive(false);
        Crosshair = GameObject.Find("Crosshair");

        //set the color to use for the highlighted hotbar slot
        ColorUtility.TryParseHtmlString("#715A53", out hotbarSlotHighlight);
        HotbarSlots[0].GetComponent<Image>().color = hotbarSlotHighlight;

        //initialize the inventory
        InitializeInventory(Items);
    }

    void Update()
    {
        //if scroll up move the active hotbar slot up
        if(Input.GetAxis("Mouse ScrollWheel") > 0f) {
            if(activeHotbarSlot == 1) {return;}

            HotbarSlots[activeHotbarSlot - 1].GetComponent<Image>().color = new Color(255f, 255f ,255f ,255f);
            activeHotbarSlot--;
            HotbarSlots[activeHotbarSlot - 1].GetComponent<Image>().color = hotbarSlotHighlight;

        //else if scroll down move the active hotbar slot down
        } else if(Input.GetAxis("Mouse ScrollWheel") < 0f) {
            if(activeHotbarSlot == 9) {return;}

            HotbarSlots[activeHotbarSlot - 1].GetComponent<Image>().color = new Color(255f, 255f ,255f ,255f);
            activeHotbarSlot++;
            HotbarSlots[activeHotbarSlot - 1].GetComponent<Image>().color = hotbarSlotHighlight;
        }

        if(Input.GetKeyDown(KeyCode.Q)) {
            if(instance.Items.Count > activeHotbarSlot - 1) {
                DropItem(instance.Items[activeHotbarSlot - 1]);
            }
        } else if(Input.GetKeyDown(KeyCode.Tab)) {
            InventoryMenu.SetActive(!InventoryMenu.activeSelf);
            GameObject.FindObjectOfType<SC_FPSController>().ToggleMovement();
            Crosshair.SetActive(!Crosshair.activeSelf);

            GameStateManager.instance.ToggleGameState(GameStateManager.GameState.InMenu);
        }
        
    }
    public void InitializeInventory(List<Item> itemsInInventory) {

        for(int i = Items.Count; i < numInventorySlots + 9; i++) {
            Items.Add(null);
            StackSizes.Add(0);
        }

        for(int i = 0; i < numInventorySlots; i++) {
            GameObject inventorySlot = Instantiate(InventorySlot, InventoryUI.transform);
            inventorySlot.GetComponent<InventoryPosition>().inventoryPosition = i + 9;
            InventorySlots.Add(inventorySlot);
        }

        UpdateInventory(itemsInInventory);
    }

    public void UpdateInventory(List<Item> itemsInInventory) {
        UpdateHotbar();

        for(int i = 9; i < numInventorySlots + 9; i++) {
            if(Items[i] == null) {
                InventorySlots[i - 9].transform.Find("Item").GetComponent<Image>().sprite = null;
                InventorySlots[i - 9].transform.Find("Item").GetComponent<Image>().color = Color.clear;
                InventorySlots[i - 9].transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "";
            } else {
                InventorySlots[i - 9].transform.Find("Item").GetComponent<Image>().sprite = Items[i].icon;
                InventorySlots[i - 9].transform.Find("Item").GetComponent<Image>().color = Color.white;
                InventorySlots[i - 9].transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = StackSizes[i].ToString();
            }
        }
    }

    public void UpdateHotbar() {
        //loop through all of the hotbar slots
        for(int i = 0; i < 9; i++) {

            //if if we have less items than hotbar slots, make the slots at the end empty
            if(Items[i] == null) {
                HotbarSlots[i].transform.Find("Icon").GetComponent<Image>().sprite = null;
                HotbarSlots[i].transform.Find("Icon").GetComponent<Image>().color = Color.clear;
                HotbarSlots[i].transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = "";
            
            //else put the information from the items into the hotbar slot
            } else {
                HotbarSlots[i].transform.Find("Icon").GetComponent<Image>().sprite = Items[i].icon;
                HotbarSlots[i].transform.Find("Icon").GetComponent<Image>().color = Color.white;
                HotbarSlots[i].transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = StackSizes[i].ToString();
            }
        }
    }

    public bool AddItem(Item itemToAdd, int amount) {
        
        //check if theres an instance of the item we want to add in our inventory currently
        bool itemInInventory = ItemInInventory(itemToAdd);
        //get all the slots in our inventory that the item we want to add holds
        List<int> indices = GetInventoryIndices(itemToAdd);

        //check if theres room in the inventory to add the items we need to add
        //if there is no room we cant add the items
        if(!CanAddItem(itemToAdd, amount)) {
            Debug.Log("No space to add the item(s)");
            return false;
        }
        Debug.Log("space in inventory");

        //if we dont have an instance of the item in our inventory
        if(!itemInInventory) {

            //store the amount of items left we need to add
            int amountLeftToAdd = amount;

            //while we have more items to add
            while(amountLeftToAdd > 0) {

                //since theres no instances of the item in our inventory we find the first empty inventory slot and add the item to it
                int inventoryIndex = Items.FindIndex((x) => x == null);
                Items[inventoryIndex] = itemToAdd;
                int currentStackSize = StackSizes[inventoryIndex];

                //if we have more items to add than can fit in the current stack we set the stack to max then subtract the difference from the amount we have left to add
                if(amountLeftToAdd >= maxStackSize - currentStackSize) {
                    StackSizes[inventoryIndex] = maxStackSize;
                    amountLeftToAdd -= (maxStackSize - currentStackSize);

                //else if theres enough room in the stack to fit the rest of the items we add them to the stack and set the amount left to add to 0 to finish adding items
                } else {
                    StackSizes[inventoryIndex] += amountLeftToAdd;
                    amountLeftToAdd = 0;
                }
            }

        //else if we do have instances of the item in the inventory we must fill up the empty stacks first
        } else {
            int amountLeftToAdd = amount;

            //loop through all of the instances of the item in our inventory and add the items to those stacks first
            for(int i = 0; i < indices.Count; i++) {
                if(amountLeftToAdd > 0) {
                    int currentStackSize = StackSizes[indices[i]];
                    if(amountLeftToAdd >= maxStackSize - currentStackSize) {
                        StackSizes[indices[i]] = maxStackSize;
                        amountLeftToAdd -= maxStackSize - currentStackSize;
                    } else {
                        StackSizes[indices[i]] += amountLeftToAdd;
                        amountLeftToAdd = 0;
                    }
                } else {
                    UpdateInventory(Items);
                    return true;
                }
            }

            //after adding to all the not full stacks if we have any items left we need to add, add them to empty inventory slots like above
            while(amountLeftToAdd > 0) {
                int inventoryIndex = Items.FindIndex((x) => x == null);
                Items[inventoryIndex] = itemToAdd;
                int currentStackSize = StackSizes[inventoryIndex];

                if(amountLeftToAdd >= maxStackSize - currentStackSize) {
                    StackSizes[inventoryIndex] = maxStackSize;
                    amountLeftToAdd -= (maxStackSize - currentStackSize);
                } else {
                    StackSizes[inventoryIndex] += amountLeftToAdd;
                    amountLeftToAdd = 0;
                }
            }
        }
        
        UpdateInventory(Items);
        return true;
    }

    
    //@ SHOULD MAYBE RESTACK ITEMS AFTER REMOVING MAYBE IDK, IT WORKS RN THO
    public bool RemoveItem(Item itemToRemove, int startIndex, int amountToRemove) {
        
        //get where the item to remove is within our inventory
        List<int> indicesOfItemInInventory = GetInventoryIndices(itemToRemove);
        int sumOfItemsInInventory = 0;

        //get the total number of the item we want to remove in our inventory 
        for(int i = 0; i < indicesOfItemInInventory.Count; i++) {
            sumOfItemsInInventory += StackSizes[indicesOfItemInInventory[i]];
        }

        //if we dont have enough items in our inventory we obviously cant remove them so we stop
        if(sumOfItemsInInventory < amountToRemove) {
            Debug.Log("Not enough items to remove in your inventory dumbass");
            return false;
        
        //else if we do have enough items in our inventory continue with the removal process
        } else {
            int numItemsToRemove = amountToRemove;
            //loop through all of the stacks of the item in our inventory
            for(int i = 0; i < indicesOfItemInInventory.Count; i++) {

                //if the stack we're looking at is before our start then move to the next stack
                if(indicesOfItemInInventory[i] < startIndex) {
                   
                } else {

                    //if the number of items in the stack is less than the number we want to remove then set it to 0 so we can remove it later and
                    //remove the stack amount from the amount we're removing
                    if(StackSizes[indicesOfItemInInventory[i]] <= numItemsToRemove) {
                        numItemsToRemove -= StackSizes[indicesOfItemInInventory[i]];
                        StackSizes[indicesOfItemInInventory[i]] = 0;

                    //else if there are more items in the stack then just remove the amount we have left to remove from the stack then remove all the
                    //items with 0 in their stack from our inventory
                    } else {
                        StackSizes[indicesOfItemInInventory[i]] -= numItemsToRemove;
                        
                        //loop through the stacks and if its empty disable it
                        for(int index = StackSizes.Count - 1; index >= 0; index--) {
                            if(StackSizes[index] == 0) {
                                Items[index] = null;
                            }
                        }


                        UpdateInventory(Items);
                        return true;
                    }
                }

            }
        }

        //i dont think i need this extra check but maybe? apparently i do because it broke everything lol

        for(int index = StackSizes.Count - 1; index >= 0; index--) {
            if(StackSizes[index] == 0) {
                Items[index] = null;
            }
        }

        UpdateInventory(Items);
        return true;
    }

    public void DropItem(Item itemToDrop) {
        //if we fail removing the item, exit so we dont drop the item on the ground
        if(!RemoveItem(itemToDrop, activeHotbarSlot - 1, 1)) { return;}

        float distanceToDrop = 5f;

        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Transform player = GameObject.Find("FPSPlayer").transform;
        Vector3 spawnPoint;

        if(Physics.Raycast(ray, out hit)) {
            Debug.Log(hit.point);
            Debug.Log(hit.distance);

            if(hit.distance < distanceToDrop) {
                spawnPoint = (ray.direction * hit.distance) + player.transform.position;
            } else {
                spawnPoint = (ray.direction * distanceToDrop) + player.transform.position;

            }

        } else {
                spawnPoint = (ray.direction * distanceToDrop) + player.transform.position;

        }


        GameObject droppedItem = Instantiate(Resources.Load(itemToDrop.prefabPath, typeof (GameObject))) as GameObject;
        droppedItem.transform.position = spawnPoint;
        
    }

    public void SwapInventorySlots(int slotA, int slotB) {
        Item tempItem = Items[slotA];
        int tempStackSize = StackSizes[slotA];

        Items[slotA] = Items[slotB];
        StackSizes[slotA] = StackSizes[slotB];
        Items[slotB] = tempItem;
        StackSizes[slotB] = tempStackSize;

        UpdateInventory(Items);
    }

    public List<int> GetInventoryIndices(Item item) {
        return Enumerable.Range(0, instance.Items.Count).Where(i => instance.Items[i] == item).ToList();
    }

    public bool ItemInInventory(Item item) {
        return instance.Items.FindAll((x) => x == item).Count > 0;

    }

    public bool CanAddItem(Item item, int amount) {
        List<int> indices = GetInventoryIndices(item);
        List<Item> emptyStacks = Items.FindAll((x) => x==null);
        if(emptyStacks.Count * maxStackSize > amount) {
            return true;
        }

        int amountToAdd = amount;
        if(indices.Count > 0) {
            for(int i = 0; i < indices.Count; i++) {
                if(StackSizes[indices[i]] < maxStackSize - amountToAdd) {
                    return true;
                } else {
                    amountToAdd -= maxStackSize - StackSizes[indices[i]];
                }
            }
        }
        return false;
    }

}
