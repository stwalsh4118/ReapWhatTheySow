using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Interact() {
        Debug.Log("Interacted with");
    }

    public virtual void Interact(Vector3 position) {
        
    }
}
