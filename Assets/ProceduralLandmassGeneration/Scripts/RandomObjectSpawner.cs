using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

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

                if(index % 16 == 0 && vertex.y > spawnableObjects[i].minHeightRange && vertex.y < spawnableObjects[i].maxHeightRange) {

                    GameObject spawnedObject = Instantiate(spawnableObjects[i].objectToSpawn, new Vector3((vertex.x + position.x) * mapGenerator.terrainData.uniformScale, (vertex.y) * mapGenerator.terrainData.uniformScale, (vertex.z + position.y) * mapGenerator.terrainData.uniformScale) , Quaternion.identity);
                    spawnedObject.transform.localScale = new Vector3(3,3,3);
                    spawnedObject.transform.eulerAngles = RandomYRotation(0f, 360f);
                    //spawnedObject.transform.parent = transform;

                }

            index++;

            }
        }

    }

    public void SpawnObjectsPoisson(Vector3[] vertices, Vector2 position, GameObject TerrainChunk) {
        Vector2 mapSize = new Vector2((float)mapGenerator.mapChunkSize, (float)mapGenerator.mapChunkSize);
        List<Vector2> spawnPoints = PoissonDiscSampling.GeneratePoints(3f, mapSize, 5);

        // foreach(Vector3 vertex in vertices) {
        //     Debug.Log(vertex);
        // }

        //Debug.Log(DateTime.Now);
        for(int i = 0; i < spawnableObjects.Count; i++) {

            foreach(Vector2 spawnPoint in spawnPoints) {
                Vector3 pointOnMesh = FindMeshHeightAtPoint(vertices, new Vector2(spawnPoint.x, spawnPoint.y));
                //Debug.Log(closestVertexHeight);
                if(pointOnMesh.y > spawnableObjects[i].minHeightRange && pointOnMesh.y < spawnableObjects[i].maxHeightRange) {
                    GameObject spawnedObject = Instantiate(spawnableObjects[i].objectToSpawn, (pointOnMesh + new Vector3(position.x, 0, position.y)) * mapGenerator.terrainData.uniformScale , Quaternion.identity);
                    //GameObject spawnedObject = Instantiate(spawnableObjects[i].objectToSpawn, new Vector3((-119) * mapGenerator.terrainData.uniformScale, closestVertexHeight * mapGenerator.terrainData.uniformScale, (119) * mapGenerator.terrainData.uniformScale) , Quaternion.identity);

                    spawnedObject.transform.localScale = new Vector3(3,3,3);
                    spawnedObject.transform.eulerAngles = RandomYRotation(0f, 360f);
                    spawnedObject.transform.parent = TerrainChunk.transform;
                }

            }
        }
        //Debug.Log(DateTime.Now);

        
    }

    public Vector3 RandomYRotation(float minRotation, float maxRotation) {
        return new Vector3(0f, UnityEngine.Random.Range(minRotation, maxRotation), 0f);
    }

    public Vector3 FindMeshHeightAtPoint(Vector3[] vertices, Vector2 point) {
        int vertexIndex = (Mathf.RoundToInt(point.x) + ((mapGenerator.mapChunkSize - 1) - Mathf.RoundToInt(point.y)) * mapGenerator.mapChunkSize);
        if(vertexIndex < 0 || vertexIndex > vertices.Length) {
            return new Vector3(0,0,0);
        }
        return vertices[vertexIndex];
    }   

    [System.Serializable]
    public class Spawnable {
        public GameObject objectToSpawn;
        public float minHeightRange;
        public float maxHeightRange;
        public float chanceToSpawn;

    }
}
