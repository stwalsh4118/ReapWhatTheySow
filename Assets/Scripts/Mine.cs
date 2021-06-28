using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    // Start is called before the first frame update

    public string minedObject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Hit() {
        if(minedObject != null) {
            SpawnMinedObject();
        }
    }

    void SpawnMinedObject() {
        GameObject minedObjectInstance = Instantiate(Resources.Load(minedObject, typeof (GameObject))) as GameObject;
        minedObjectInstance.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        Rigidbody rb = minedObjectInstance.GetComponent<Rigidbody>();
        Vector3 OUS = Random.onUnitSphere;
        OUS.y = Mathf.Abs(OUS.y);
        rb.AddRelativeForce(OUS*20);
    }
}
