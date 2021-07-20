using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : Interactable
{
    public Item resource;
    public bool spawnItemsAroundOnGeneration;
    public int initialSpawnAmount;
    public float initialSpawnRadius;
    // Start is called before the first frame update
    void Start()
    {
        InitialSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialSpawn() {
        if(spawnItemsAroundOnGeneration) {
            for(int i = 0; i < initialSpawnAmount; i++) {
                Vector2 pointOnUnitCircle = Random.insideUnitCircle.normalized;
                if(pointOnUnitCircle.x == 0 && pointOnUnitCircle.y == 0) {
                    pointOnUnitCircle = Vector2.one;
                }

                Vector3 spawnPoint = new Vector3(pointOnUnitCircle.x * initialSpawnRadius, 0, pointOnUnitCircle.y * initialSpawnRadius) + transform.position;

                GameObject spawnedObject = Instantiate(Resources.Load(resource.prefabPath, typeof (GameObject))) as GameObject;
                spawnedObject.transform.position = spawnPoint;
            }
        }
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

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, initialSpawnRadius);
    }
}
