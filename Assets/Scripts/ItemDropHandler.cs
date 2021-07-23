using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(gameObject.GetComponent<InventoryPosition>().inventoryPosition);
        Inventory.instance.SwapInventorySlots(Inventory.instance.mostRecentDragStart, gameObject.GetComponent<InventoryPosition>().inventoryPosition);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
