using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : Interactable
{
    public Item resource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact(Vector3 hitPosition)
    {
        GameObject spawnedResource = Instantiate(Resources.Load(resource.prefabPath, typeof (GameObject))) as GameObject;

        //put the spawned object above the spawner object
        spawnedResource.transform.position = hitPosition;
        spawnedResource.transform.eulerAngles = new Vector3(Random.Range(0f,360f), Random.Range(0f,360f), Random.Range(0f,360f));

        //give the spawned object a random force up and out to shoot it into the air a little
        Rigidbody rb = spawnedResource.GetComponent<Rigidbody>();
        Vector3 OUS = Random.onUnitSphere;
        OUS.y = Mathf.Abs(OUS.y);
        rb.AddRelativeForce(OUS*20);
    }
}
