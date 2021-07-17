using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode {
        NoiseMap,
        Mesh,
        FalloffMap
    }
    public DrawMode drawMode;

    [Range(0,6)]
    public int editorLOD;
 
    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;
    public Material terrainMaterial;

    public bool autoUpdate;

    float[,] falloffMap;
    
    //queues that hold the data we get back from the threads
    private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void OnValuesUpdated() {
        if(!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    private void OnTextureValuesUpdated() {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    //creating static instance of mapChunkSize so that we can set the chunk size depending on if we're using flat shading or not
    public int mapChunkSize {
        get {
            if(terrainData.useFlatShading) {
                return 95;
            } else {
                return 239;
            }
        }
    }


    //gets the data from the GenerateMapData method then draws that data depending on the mode
    public void DrawMapInEditor() {

        MapData mapData = GenerateMapData(Vector2.zero);

        //display the different modes depending on which one is selected
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        } else if(drawMode == DrawMode.Mesh) {
            display.DrawMesh(ProceduralMeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorLOD, terrainData.useFlatShading));
        } else if(drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    //generates the map that we can see from the noise map
    private MapData GenerateMapData(Vector2 center) {
        //generate our noise map from the perlin noise
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

        //if we are using the falloff map then we subtract the falloff map from the noise map to get an island surrounded by water
        if(terrainData.useFalloff) { 

            if(falloffMap == null) {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            }

            for(int y = 0; y < mapChunkSize + 2; y++) {
                for(int x = 0; x < mapChunkSize + 2; x++) {                
                    if(terrainData.useFalloff) {
                        noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - falloffMap[x,y]);
                    }
                }
            }
        }
        
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        return new MapData(noiseMap);
    }
    
    //method we call when we want to generate a part of the map
    //takes in an "Action" which is the method that the thread will run after it is finished
    public void RequestMapData(Vector2 center, Action<MapData> callback) {

        //initialize a ThreadStart that designates what the thread will being doing when it starts
        ThreadStart threadStart = delegate {

            //we designate our thread to run the MapDataThread method with a callback to run after it is finished
            MapDataThread(center, callback);
        };

        //generate and start the thread to run
        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback) {
        //generate the map data within the thread
        MapData mapData = GenerateMapData(center);

        //we use the lock keyword to make it so that our queue will not be updated twice at the same time which is possible when threading 
        //so when a thread reaches this point, before executing its code it must wait its turn if another thread is executing code on the queue
        lock(mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    //works the same as Request Map Data from above
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread (mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
        //generate the mesh data within the thread
        MeshData meshData = ProceduralMeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);

        //we use the lock keyword to make it so that our queue will not be updated twice at the same time which is possible when threading 
        //so when a thread reaches this point, before executing its code it must wait its turn if another thread is executing code on the queue
        lock(meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }
    
    private void Update() {

        //if we have MapData that has been generated by a thread and is waiting to be used in the queue we loop through the queue and act on the data with the given callback
        //heads up for me reading this later: if youre threading using an enumerated data type (like a queue) using foreach will throw errors as it tries to enumerate over a changing queue
        //  if the queue changes due to a thread (which is what i tried at first, idk if that those errors are actually doing anything because it seemed to be doing the callbacks 
        //  just fine but i switched to just basic for loop and it stopped throwing errors ¯\_(ツ)_/¯)
        if(mapDataThreadInfoQueue.Count > 0) {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0) {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //called when a value of the script in changed in the editor, we can use it to clamp values to where we want
    private void OnValidate() {
        
        if(terrainData != null) {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }

        if(noiseData != null) {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }

        if(textureData != null) {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

    //struct that will hold the generated data from our threads that we will act upon within Unity's main thread after the map generation thread has completed
    struct MapThreadInfo<T> {

        //readonly keyword so that our data in immutable
        //we are also using this for generating map data and mesh data so we can make it generic with the T type so that we can use it with MapData and MeshData
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
    
 }


//struct to hold our map data instead of just passing the values
public struct MapData {
    public readonly float[,] heightMap;

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
