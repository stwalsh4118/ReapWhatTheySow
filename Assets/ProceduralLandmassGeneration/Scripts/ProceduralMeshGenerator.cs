using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //calculate top left coordinates such that the center of our mesh will be (0,0)
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //initialize our mesh's data
        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        //loop through every vertex in our grid
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                
                //add x since were going left to right, subtract y since were going top to bottom
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x,y], topLeftZ - y);
                //uvs are data that tell us were each vertex is in relation to the rest of the map, in the form of an (x,y) that is a percentage from 0 to 1
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

                //if our current vertex isnt along the bottom or right most edge, then add 2 triangles that make up the quad with our vertext in the top left
                //
                //  1 -- 2 -- 3
                //  | \\ | \\ |
                //  4 -- 5 -- 6
                //  | \\ | \\ |
                //  7 -- 8 -- 9
                //
                //  so in the above case with 9 vertices we would add triangles when were at vertices 1, 2, 4, and 5
                //  and when we generalize the addition of triangles to work generalized meshes the vertices we add to the triangles works out to:
                //
                //      triangle 1 (index, index + width + 1, index + width)
                //      triangle 2 (index + width + 1, index, index + 1)
                //
                //  these indices that we add to the triangle array can be in any order as long as they go "clock-wise" around the triangle (for unity at least)
                //
                //  e.g. a valid triangle would include indices 1, 2, and 5, or 2, 5, 1. Same indices in a different order makes the same triangle,
                //       as long as they are put in the array in a clock-wise order
                if((x < width - 1) && (y < height - 1)) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    int triangleIndex;
    //constructor
    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshHeight * meshWidth];
        uvs = new Vector2[meshHeight * meshWidth];
        triangles = new int[(meshHeight - 1)*(meshWidth - 1)*6];
    }

    //QOL method to aid in adding triangles to our triangles array
    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    //creates an actual mesh from our mesh data
    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
    
}
