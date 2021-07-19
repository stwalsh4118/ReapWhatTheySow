using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnObject : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
     
     void Update()
     {   
        //get a ray coming out from the mouse (in our case the middle of the screen)
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if the ray hits something
        if(Physics.Raycast(ray, out hit))
        {   
            //on mouse click if the ray is currently hitting something do something
            if(Input.GetMouseButtonDown(0)) {
                print(hit.collider.name);

                if(hit.transform.GetComponent<Interactable>()) {
                    Vector3 closerHitPoint = ((hit.distance * .95f) * ray.direction) + transform.position; 
                    hit.transform.SendMessage("Interact", closerHitPoint);
                }
            }
         }
     }
}
