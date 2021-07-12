using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    //creates a noise map
    //params: 
    //  int mapWidth: width of the map
    //  int mapHeight: height of the map
    //  float scale: number used to scale the perlin nosie
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale) {

        //initialize noiseMap which is just a 2d array of floats
        float[,] noiseMap = new float[mapWidth,mapHeight];

        //clamp the scale so we dont divide by 0, or get negative scales
        if(scale <= 0) {
            scale = 0.0001f;
        }

        //loop through the coordinates givin with our range of heigh and width and set each coordinate of our nosie map with a value from the perlin noise
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapHeight; x++) {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x,y] = perlinValue;
            }
        }

        return noiseMap;
    }

}
