using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryOtherOnContact : MonoBehaviour
{
    private void OnCollisionEnter(Collision other) {
        Debug.Log("Destroying " + other.transform.name);
        Destroy(other.gameObject);
    }
}
