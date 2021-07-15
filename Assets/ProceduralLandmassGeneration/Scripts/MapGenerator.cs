using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode {
        NoiseMap,
        ColorMap,
        Mesh
    }
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public float noiseScale;
    public int octaves;

    //range attribute clamps the persistance value between 0 and 1 and creates a slider in the editor
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;


    public bool autoUpdate;
    public TerrainType[] regions;

    //generates the map that we can see from the noise map
    public void GenerateMap() {
        //generate our noise map from the perlin noise
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        //populate our colorMap from our noise map using the colors and value ranges from our preset "regions"
        for(int y = 0; y < mapChunkSize; y++) {
            for(int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x,y];

                for(int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height) {
                        
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        //display the different modes depending on which one is selected
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if(drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        } else if(drawMode == DrawMode.Mesh) {
            display.DrawMesh(ProceduralMeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
    }

    //called when a value of the script in changed in the editor, we can use it to clamp values to where we want
    private void OnValidate() {
        if(lacunarity < 1) {
            lacunarity = 1;
        }
        if(octaves < 0) {
            octaves = 0;
        }

    }

    //struct to hold the data that makes up a "region", e.g. the name we give the region, the max height range where the regions color takes affect, and the color to apply to the color map,
    //if the noiseMap point is within that region's range
    [System.Serializable]
    public struct TerrainType {
        public string name;
        public float height;
        public Color color;
    }
 }
