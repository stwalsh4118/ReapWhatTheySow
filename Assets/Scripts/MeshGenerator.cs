using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    //create mesh that we will be generating onto
    Mesh mesh;

    //vertices and triangles arrays that make up the parts of the mesh that we can render
    Vector3[] vertices;
    int[] triangles;

    //size of the mesh
    public int xSize = 20;
    public int zSize = 20;

    //scale of the perlin noise used to generate the random heights, higher scale = more zoomed out
    public float scale = 1;

    //amplitude of the output of the perlin noise function, higher amplitude = higher peaks
    public float amplitude = 10;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }
    
    // Update is called once per frame
    void Update()
    {
    
    }

    void CreateShape() {
        

        //initialize vertices array
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        //loop through all of the vertices and give them points on the grid, get y coordinate from the perlin noise to get "height"
        for(int i = 0, z = 0; z <= zSize; z++) {
            for(int x = 0; x <=  xSize; x++) {

                float perlinXCoord = (float)x / xSize * scale;
                float perlinZCoord = (float)z / zSize * scale;

                float y = Mathf.PerlinNoise(perlinXCoord, perlinZCoord) * amplitude;

                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        //initialize triangles array, each "triangle" is a series of 3 numbers which corresponds to the vertices that makes up the triangle in clockwise order
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        //loop through the vertices and add triangles to the array
        for(int z = 0; z < zSize; z++) {

            for(int x = 0; x < xSize; x++) {

                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
        
            }
            vert++;
        }

        

       

    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private void OnDrawGizmos() {

        if(vertices==null) {
            return;
        }

        for(int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }

}
