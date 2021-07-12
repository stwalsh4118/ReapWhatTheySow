using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invPanel = transform as RectTransform;

        //if after dragging an item it isnt in its "slot" anymore then do something
        if(!RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition)) {
            GameObject minedObjectInstance = Instantiate(Resources.Load("Prefabs/Copper", typeof (GameObject))) as GameObject;

            Transform player = GameObject.Find("FPSPlayer").transform;

            Vector3 forward = Vector3.Normalize(player.forward);
            forward.y = 0;
            Vector3 spawnPoint = player.position +  forward * 3f;
            //put the spawned object above the spawner object
            minedObjectInstance.transform.position = spawnPoint;
        } else {
            Debug.Log("Item not dropped");
        }
        
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
