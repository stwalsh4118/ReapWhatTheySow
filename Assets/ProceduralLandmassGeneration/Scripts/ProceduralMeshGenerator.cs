using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {

        //threading messes with the values from the passed in height curve so we just create a new height curve with all of the same values *within the thread* and then the curve works
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //calculate top left coordinates such that the center of our mesh will be (0,0)
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //we clamped the range we can input our level of detail to 0-6 so we multiply it by 2 (or make it 1 if 0) so that we have our factors of 240
        int meshSimplificationIncrement = (levelOfDetail==0) ? 1 : levelOfDetail * 2;
        //this calculates the number of vertices per line, considering the amout that we simplified the mesh by
        int verticiesPerLine = ((width - 1) / meshSimplificationIncrement) + 1;

        //initialize our mesh's data
        MeshData meshData = new MeshData(verticiesPerLine, verticiesPerLine);
        int vertexIndex = 0;

        //loop through every vertex in our grid
        for(int y = 0; y < height; y += meshSimplificationIncrement) {
            for(int x = 0; x < width; x += meshSimplificationIncrement) {
                
                //add x since were going left to right, subtract y since were going top to bottom
                //  We are also using an animation curve to transform our vertex height values into the values on the curve,
                //  e.g. making height values below .4 go to around 0 so our "water" is flat
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
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
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.AddTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

//a class for easily holding our mesh's data and creating a mesh from said data
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
