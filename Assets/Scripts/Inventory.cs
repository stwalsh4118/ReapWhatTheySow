using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{

    public List<GameObject> HotbarSlots;
    public List<Item> Items;
    public List<int> StackSizes;
    [SerializeField]
    public List<(Item item, int stackSize)> iten;
    public int maxItems = 20;
    public int numHotbarSlots = 9;
    public int activeHotbarSlot = 1;
    Color hotbarSlotHighlight = new Color();
    public static Inventory instance;


    void Start()
    {
        //set the static instance
        instance = this;

        //set the color to use for the highlighted hotbar slot
        ColorUtility.TryParseHtmlString("#715A53", out hotbarSlotHighlight);
        HotbarSlots[0].GetComponent<Image>().color = hotbarSlotHighlight;

        //initialize the hotbar
        UpdateHotbar();
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

    public void AddItem(Item itemToAdd) {

        //find the item we picked up in our inventory
        Item pickedUpItem = instance.Items.Find((x) => x == itemToAdd);

        //if we didnt find the item in our inventory add it to the inventory
        if(!pickedUpItem) {
            Debug.Log("Item not in inventory");
            instance.Items.Add(itemToAdd);
            instance.StackSizes.Add(1);
            UpdateHotbar();

        //if we did find the item in our inventory just add to its stacksize
        } else {
            Debug.Log("Item in inventory");
            int positionInInventory = instance.Items.IndexOf(pickedUpItem);
            Debug.Log(positionInInventory.ToString() + " position of object in inventory");
            Debug.Log("Number of items in inventory before adding " + StackSizes[positionInInventory].ToString());
            StackSizes[positionInInventory]++;
            Debug.Log("Number of items in inventory after adding " + StackSizes[positionInInventory].ToString());
            UpdateHotbar();
        }
    }

}
