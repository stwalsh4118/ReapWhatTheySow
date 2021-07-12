using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {

        //initialize a texture so that we can put the color from our color array onto the pixels
        Texture2D texture = new Texture2D(width, height);
        //Point filter to make more crisp pixel texture
        texture.filterMode = FilterMode.Point;
        //clamp wrap mode so that we dont see bleeding from other side on the edges
        texture.wrapMode = TextureWrapMode.Clamp;
        //apply the colors from our color map onto the pixels of the texture
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //initialize our colorMap that will house the colors that we set the pixels to
        Color[] colorMap = new Color[width*height];

        //loop through all of the points in the noise map and generate a grey-scale color from the noise map values
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y]);
            }
        }

        //create a texture from our color map and return it
        return TextureFromColorMap(colorMap, width, height);
    }
}
