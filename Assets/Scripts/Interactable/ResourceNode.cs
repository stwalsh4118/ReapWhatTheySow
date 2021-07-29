using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : Interactable
{
    public List<Resource> resources;
    public bool spawnItemsAroundOnGeneration;
    [Range(.001f, 1000)]
    public float initialSpawnAmount;
    public float initialSpawnRadius;
    // Start is called before the first frame update
    void Start()
    {
        InitialSpawn();
    }


    public void InitialSpawn() {
        if(spawnItemsAroundOnGeneration) {

            if(initialSpawnAmount < 1) {
                if(Random.Range(0f , 1f) < initialSpawnAmount) {
                    Vector2 pointOnUnitCircle = Random.insideUnitCircle.normalized;
                    if(pointOnUnitCircle.x == 0 && pointOnUnitCircle.y == 0) {
                        pointOnUnitCircle = Vector2.one;
                    }

                    Vector3 spawnPoint = new Vector3(pointOnUnitCircle.x * initialSpawnRadius, 0, pointOnUnitCircle.y * initialSpawnRadius) + transform.position;

                    GameObject spawnedObject = Instantiate(Resources.Load(resources[0].item.prefabPath, typeof (GameObject))) as GameObject;
                    spawnedObject.transform.position = spawnPoint;
                    }
            } else {

                for(int i = 0; i < (int)initialSpawnAmount; i++) {
                    Vector2 pointOnUnitCircle = Random.insideUnitCircle.normalized;
                    if(pointOnUnitCircle.x == 0 && pointOnUnitCircle.y == 0) {
                        pointOnUnitCircle = Vector2.one;
                    }

                    Vector3 spawnPoint = new Vector3(pointOnUnitCircle.x * initialSpawnRadius, 0, pointOnUnitCircle.y * initialSpawnRadius) + transform.position;

                    GameObject spawnedObject = Instantiate(Resources.Load(resources[0].item.prefabPath, typeof (GameObject))) as GameObject;
                    spawnedObject.transform.position = spawnPoint;
                }
            }
        }
    }

    public override void Interact(Vector3 hitPosition, int equipmentTier, equipmentType equippedType)
    {
        Resource resource = SelectResource(resources, equipmentTier, equippedType);
        if(resource == null) {return ;}
        GameObject spawnedResource = Instantiate(Resources.Load(resource.item.prefabPath, typeof (GameObject))) as GameObject;

        //put the spawned object above the spawner object
        spawnedResource.transform.position = hitPosition;
        spawnedResource.transform.eulerAngles = new Vector3(Random.Range(0f,360f), Random.Range(0f,360f), Random.Range(0f,360f));

        //give the spawned object a random force up and out to shoot it into the air a little
        Rigidbody rb = spawnedResource.GetComponent<Rigidbody>();
        Vector3 OUS = Random.onUnitSphere;
        OUS.y = Mathf.Abs(OUS.y);
        rb.AddRelativeForce(OUS*20);
    }

    public Resource SelectResource(List<Resource> resources, int equipmentTier, equipmentType equippedType) {
        float weightSum = 0;
        List<Resource> objectsInEquipmentRange = new List<Resource>();
        for(int i = 0; i < resources.Count; i++) {
            Debug.Log(resources[i].item.itemName + " " + resources[i].item.itemTier.ToString());
            if(resources[i].item.itemTier == 0 || resources[i].item.itemTier <= equipmentTier && (resources[i].item.equipmentToCollect == equippedType || equippedType == equipmentType.ALL || resources[i].item.equipmentToCollect == equipmentType.ALL)) {
                objectsInEquipmentRange.Add(resources[i]);
                Debug.Log(resources[i].item.itemName);
            }
        }

        if(objectsInEquipmentRange.Count == 0) {
            return null;
        }

        for(int i = 0; i < objectsInEquipmentRange.Count; i++) {
            weightSum += objectsInEquipmentRange[i].collectionWeight;
        }
        float random = UnityEngine.Random.value;
        objectsInEquipmentRange.Sort(delegate(Resource a, Resource b) {
            return a.collectionWeight.CompareTo(b.collectionWeight);
        });

        for(int i = 0; i < objectsInEquipmentRange.Count; i++) {
            if(objectsInEquipmentRange[i].collectionWeight / weightSum > random) {
                return objectsInEquipmentRange[i];
            }
        }
        return objectsInEquipmentRange[objectsInEquipmentRange.Count - 1];
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, initialSpawnRadius);
    }

    [System.Serializable]
    public class Resource {
        public ResourceItem item;
        public float collectionWeight;
    }
}
