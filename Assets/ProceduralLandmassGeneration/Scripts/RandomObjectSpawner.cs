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

    public void SpawnObjectsPoisson(Vector3[] vertices, Vector3[] normals, Vector2 position, GameObject TerrainChunk) {
        Vector2 mapSize = new Vector2((float)mapGenerator.mapChunkSize, (float)mapGenerator.mapChunkSize);
        List<Vector2> spawnPoints = PoissonDiscSampling.GeneratePoints(3f, mapSize, 5);
        List<Vector2> pointsAlreadySpawned = new List<Vector2>();
        SelectSpawnable(spawnableObjects, 0);


        foreach(Vector2 spawnPoint in spawnPoints) {
            Vector3 pointOnMesh = FindMeshHeightAtPoint(vertices, spawnPoint);
            Spawnable selected = SelectSpawnable(spawnableObjects, pointOnMesh.y);
            if(selected != null) {
                GameObject spawnedObject = Instantiate(selected.objectToSpawn, (pointOnMesh + new Vector3(position.x, -.1f, position.y)) * mapGenerator.terrainData.uniformScale, Quaternion.identity);
                spawnedObject.transform.localScale = new Vector3(3,3,3);
                spawnedObject.transform.eulerAngles = RandomYRotation(0f, 360f);
                spawnedObject.transform.SetParent(TerrainChunk.transform);
            }

        }
        
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

    public Vector3 FindVertexNormalAtPoint(Vector3[] normals, Vector2 point) {
        int vertexIndex = (Mathf.RoundToInt(point.x) + ((mapGenerator.mapChunkSize - 1) - Mathf.RoundToInt(point.y)) * mapGenerator.mapChunkSize);
        if(vertexIndex < 0 || vertexIndex > normals.Length) {
            return new Vector3(0,0,0);
        }
        return normals[vertexIndex];
    }   

    public Spawnable SelectSpawnable(List<Spawnable> spawnables, float height) {
        float weightSum = 0;
        List<Spawnable> objectsInHeightRange = new List<Spawnable>();
        for(int i = 0; i < spawnables.Count; i++) {
            if(spawnables[i].minHeightRange <= height && spawnables[i].maxHeightRange >= height) {
                objectsInHeightRange.Add(spawnables[i]);
            }
        }

        if(objectsInHeightRange.Count == 0) {
            return null;
        }

        for(int i = 0; i < objectsInHeightRange.Count; i++) {
            weightSum += objectsInHeightRange[i].spawnWeight;
        }
        float random = UnityEngine.Random.value;
        objectsInHeightRange.Sort(delegate(Spawnable a, Spawnable b) {
            return a.spawnWeight.CompareTo(b.spawnWeight);
        });

        for(int i = 0; i < objectsInHeightRange.Count; i++) {
            if(objectsInHeightRange[i].spawnWeight / weightSum > random) {
                return objectsInHeightRange[i];
            }
        }
        return objectsInHeightRange[objectsInHeightRange.Count - 1];
    }

    [System.Serializable]
    public class Spawnable {
        public GameObject objectToSpawn;
        public float minHeightRange;
        public float maxHeightRange;
        public float spawnWeight;

    }
}
