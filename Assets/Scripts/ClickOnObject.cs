using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnObject : MonoBehaviour
{
    Ray ray;
     RaycastHit hit;
     
     void Update()
     {
         ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         if(Physics.Raycast(ray, out hit))
         {
             if(Input.GetMouseButtonDown(0)) {
                print(hit.collider.name);
                if(hit.transform.tag == "Mineable") {
                    hit.transform.SendMessage("Hit");
                }

                //if(x)
             }
         }
     }
}
