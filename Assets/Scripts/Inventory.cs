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
    public List<Item> Items;
    public List<int> StackSizes;
    int maxStackSize = 64;
    public int numInventorySlots = 35;
    public int numHotbarSlots = 9;
    public int activeHotbarSlot = 1;
    Color hotbarSlotHighlight = new Color();
    public static Inventory instance;


    void Start()
    {
        //set the static instance
        instance = this;

        InventoryMenu.SetActive(false);

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
            if(instance.Items.Count > 0) {
                RemoveItem(instance.Items[activeHotbarSlot - 1], activeHotbarSlot - 1, true);
            }
        } else if(Input.GetKeyDown(KeyCode.Tab)) {
            InventoryMenu.SetActive(!InventoryMenu.activeSelf);
        }
        
    }

    public void InitializeInventory(List<Item> itemsInInventory) {

        for(int i = 0; i < numInventorySlots; i++) {
            GameObject inventorySlot = Instantiate(InventorySlot, InventoryUI.transform);
            InventorySlots.Add(inventorySlot);
        }

        UpdateInventory(itemsInInventory);
    }

    public void UpdateInventory(List<Item> itemsInInventory) {
        UpdateHotbar();

        for(int i = 9; i < numInventorySlots + 9; i++) {
            if(Items.Count <= i) {
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
            if(Items.Count <= i) {
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

    public bool AddItem(Item itemToAdd) {

        List<Item> allStacksInInventory = instance.Items.FindAll((x) => x == itemToAdd);
        List<int> indices = Enumerable.Range(0, instance.Items.Count).Where(i => instance.Items[i] == itemToAdd).ToList();

        if(Items.Count == numInventorySlots + 9 && StackSizes.Last() == maxStackSize) {
            Debug.Log("Inventory full dumbass");
            return false;
        }

        foreach(int index in indices) {
            Debug.Log(index);
        }

        if(allStacksInInventory.Count == 0) {
            Debug.Log("Item not in inventory");
            instance.Items.Add(itemToAdd);
            instance.StackSizes.Add(1);
            UpdateInventory(Items);

            return true;
        } else {
            Debug.Log("Item in inventory");

            foreach(int index in indices) {
                if(StackSizes[index] != maxStackSize) {
                    Debug.Log("Adding item to stack");
                    StackSizes[index]++;
                    UpdateInventory(Items);

                    return true; 
                }
            }

            Debug.Log("All stacks at max stack size, must add new stack");

            Debug.Log("Adding new stack");
            instance.Items.Add(itemToAdd);
            instance.StackSizes.Add(1);
            UpdateInventory(Items);
        }

        return true;
    }

    public void RemoveItem(Item itemToRemove, int inventoryIndex, bool dropItem) {

        if(itemToRemove != Items[inventoryIndex]) {
            Debug.Log("You probably don't want to remove this item dumbass");
        } else {
            StackSizes[inventoryIndex]--;
            if(StackSizes[inventoryIndex] <= 0) {
                StackSizes.RemoveAt(inventoryIndex);
                instance.Items.RemoveAt(inventoryIndex);
            }
            if(dropItem) {
                DropItem(itemToRemove);
            }
        }
        UpdateInventory(Items);
    }

    public void DropItem(Item itemToDrop) {
        GameObject droppedItem = Instantiate(Resources.Load(itemToDrop.prefabPath, typeof (GameObject))) as GameObject;

        Transform player = GameObject.Find("FPSPlayer").transform;

        Vector3 forward = Vector3.Normalize(player.forward);
        forward.y = 0;
        Vector3 spawnPoint = player.position +  forward * 3f;
        //put the spawned object above the spawner object
        droppedItem.transform.position = spawnPoint;
    }

}
