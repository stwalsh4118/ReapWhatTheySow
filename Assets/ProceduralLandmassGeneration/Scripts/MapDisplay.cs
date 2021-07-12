using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    //draw the noise map onto a texture that we can put on a plane
    //params:
    //  float[,] noiseMap: a 2d float array that has percent values from 0 to 1, e.g, .23, that represent the noise we got from the perlin noise generator to use as our map
    public void DrawNoiseMap(float[,] noiseMap) {
        
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        //initialize texture that we will be drawing our map onto
        Texture2D texture = new Texture2D(width, height);

        //initialize our colorMap that will house the colors that we set the pixels to
        Color[] colorMap = new Color[width*height];

        //loop through all of the points in the noise map and generate a grey-scale color from the noise map values
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
            }
        }

        //apply the colorMap that was generated to the texture
        texture.SetPixels(colorMap);
        texture.Apply();

        //render the texture and set its size to the width and height given
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
