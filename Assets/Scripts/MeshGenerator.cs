using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 20;
    public int zSize = 20;
    public float scale = 1;
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
        
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        
        for(int i = 0, z = 0; z <= zSize; z++) {
            for(int x = 0; x <=  xSize; x++) {

                float perlinXCoord = (float)x / xSize * scale;
                float perlinZCoord = (float)z / zSize * scale;

                float y = Mathf.PerlinNoise(perlinXCoord, perlinZCoord) * amplitude;

                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

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
