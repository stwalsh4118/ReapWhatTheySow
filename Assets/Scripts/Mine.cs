using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    // Start is called before the first frame update

    public Item minedObject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Hit() {
        //check that there is an object set to beable to mine just incase
        if(minedObject != null) {
            SpawnMinedObject();
        }
    }


    void SpawnMinedObject() {

        //instantiate a prefab from the resources folder
        GameObject minedObjectInstance = Instantiate(Resources.Load(minedObject.prefabPath, typeof (GameObject))) as GameObject;

        //put the spawned object above the spawner object
        minedObjectInstance.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        //give the spawned object a random force up and out to shoot it into the air a little
        Rigidbody rb = minedObjectInstance.GetComponent<Rigidbody>();
        Vector3 OUS = Random.onUnitSphere;
        OUS.y = Mathf.Abs(OUS.y);
        rb.AddRelativeForce(OUS*20);
    }
}
