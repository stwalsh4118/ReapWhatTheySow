using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{   
    public List<Spawnable> spawnableObjects;
    public static RandomObjectSpawner instance;
    public MapGenerator mapGenerator;

    private void Awake() {
        instance = this;
    }

    public void SpawnObjects(Vector3[] vertices, Vector2 position) {
        int index = 0;
        for(int i = 0; i < spawnableObjects.Count; i++) {

            foreach(Vector3 vertex in vertices) {

                if(index % 4 == 0 && vertex.y > spawnableObjects[i].minHeightRange && vertex.y < spawnableObjects[i].maxHeightRange) {

                    GameObject spawnedObject = Instantiate(spawnableObjects[i].objectToSpawn, new Vector3((vertex.x + position.x) * mapGenerator.terrainData.uniformScale, (vertex.y) * mapGenerator.terrainData.uniformScale, (vertex.z + position.y) * mapGenerator.terrainData.uniformScale) , Quaternion.identity);
                    spawnedObject.transform.localScale = new Vector3(3,3,3);
                    spawnedObject.transform.eulerAngles = RandomYRotation(0f, 360f);
                    //spawnedObject.transform.parent = transform;

                }

            index++;

            }
        }

    }

    public Vector3 RandomYRotation(float minRotation, float maxRotation) {
        return new Vector3(0f, Random.Range(minRotation, maxRotation), 0f);
    }

    [System.Serializable]
    public class Spawnable {
        public GameObject objectToSpawn;
        public float minHeightRange;
        public float maxHeightRange;
        public float chanceToSpawn;

    }
}
