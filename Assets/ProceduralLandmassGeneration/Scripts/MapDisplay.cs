using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    //draw a texture onto our screen
    //params:
    //  Texture2D texture: the texture that we will draw onto the screen
    public void DrawTexture(Texture2D texture) {

        //render the texture and set its size to the width and height given
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture) {

        //set the mesh that we generate to the mesh in our scene
        meshFilter.sharedMesh = meshData.CreateMesh();
        //set the texture to the mesh to draw the colors
        meshRenderer.sharedMaterial.mainTexture = texture;
    }   
}
