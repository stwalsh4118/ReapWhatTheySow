using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        
        //if we collided with a dropped item
        if(other.gameObject.tag == "Item") {
            Debug.Log("on trigger enter");
            Inventory.instance.AddItem(other.gameObject.GetComponent<DroppedItem>().item);
            Destroy(other.gameObject);
        }
    }
}
