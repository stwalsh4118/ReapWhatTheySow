using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public enum NormalizeMode {
        Local,
        Global
    };

    //creates a noise map
    //params: 
    //  int mapWidth (>0): width of the map
    //  int mapHeight (>0): height of the map
    //  int seed: user entered number that determines the random space we pick in our noise
    //  float scale (>0): number used to scale the perlin noise
    //  int octaves (>=0): number of octaves used to manipulate the noise
    //  float persistance (0-1): decreases the affect of the amplitude after each octave
    //  float lacunarity (>1): increases the affect of the frequency after each octave
    //  Vector2 offset: user entered coordinate that scrolls the noise in the x or y directions 
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {

        //initialize noiseMap which is just a 2d array of floats
        float[,] noiseMap = new float[mapWidth,mapHeight];

        //create random number generator with a given seed so we can get the same noise map if we want to
        System.Random prng = new System.Random(seed);

        //we want to sample our octaves from different places so we create offsets for each octave
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;



        //generate the offsets with our seeded number generator, also we can give it a set offset and scroll manually through the noise
        for(int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            //calculate the max possible height that our noise can produce so that we can normalize the whole map using that and and estimation
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        //clamp the scale so we dont divide by 0, or get negative scales
        if(scale <= 0) {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        //calculate the center of our noise map space so when we zoom with the noise scale it zooms from the center and not the top right
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //loop through the coordinates givin with our range of heigh and width and set each coordinate of our nosie map with a value from the perlin noise
        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {
                
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                //loop through all of the octaves we want to use and add the value of the noise at that octave to our aggregate noise height for the point
                for(int i = 0; i < octaves; i++) {

                    //the higher the frequency the further apart the sample values will be which means the height will change more rapidly
                    //if we affect our offsets by our scale and frequency then we will get consistent sampling when changing said offsets
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    //multiplying by 2 and then subtracting 1 changes our range of perlin noise from 0 to 1 -> -1 to 1 to have our noise sometimes subtract from the noiseheight
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    //persistance is within 0-1 so for each octave the amplitude decreases
                    amplitude *= persistance;

                    //lacunarity should be >1 so the frequency increases for each octave
                    frequency *= lacunarity;
                }

                //save the local min and max noise height that was generated so we can normalize our noise map later
                if(noiseHeight > maxLocalNoiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
                } else if(noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight;
            }
        }

        for(int y = 0; y < mapHeight; y++) {
            for(int x = 0; x < mapWidth; x++) {

                //normalize the noise map back into the 0 to 1 range
                //(only really works if we generate the whole map at once, as it leaves seams if we generate the world chunks at a time
                //  since the min and max noise height will be different for each chunk)
                if(normalizeMode == NormalizeMode.Local) {
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);
                } else {
                    //if we are generating with chunks in a "global" sense we must estimate the normalization for the whole map 
                    float normalizedHeight = (noiseMap[x,y] + 1) / (2f * maxPossibleHeight / 1.85f);
                    noiseMap[x,y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }

}
