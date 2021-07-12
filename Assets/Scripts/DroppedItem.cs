using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public bool onConveyor = false;
    public Item item;

    public float pickUpRadius = 2f;

    private void Start() {
        GetComponent<SphereCollider>().radius = pickUpRadius;
    }
}
