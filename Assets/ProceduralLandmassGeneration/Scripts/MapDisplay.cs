using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;

    //draw a texture onto our screen
    //params:
    //  Texture2D texture: the texture that we will draw onto the screen
    public void DrawTexture(Texture2D texture) {

        //render the texture and set its size to the width and height given
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
