using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {

        //threading messes with the values from the passed in height curve so we just create a new height curve with all of the same values *within the thread* and then the curve works
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        //we clamped the range we can input our level of detail to 0-6 so we multiply it by 2 (or make it 1 if 0) so that we have our factors of 240
        int meshSimplificationIncrement = (levelOfDetail==0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        //calculate top left coordinates such that the center of our mesh will be (0,0)
        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        
        //this calculates the number of vertices per line, considering the amout that we simplified the mesh by
        int verticiesPerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;

        //initialize our mesh's data
        MeshData meshData = new MeshData(verticiesPerLine);

        //create vertex map that includes a border around our mesh size
        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        //loop through all of the vertices within our given LOD range
        for(int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
            for(int x = 0; x < borderedSize; x += meshSimplificationIncrement) {

                //check if the vertex we are at in within the border
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                //if it is within the border then we set the vertex in the map to be a negative number then decrement
                if(isBorderVertex) {
                    vertexIndicesMap[x,y] = borderVertexIndex;
                    borderVertexIndex--;
                
                //else if it isnt a border vertex then set it to a positive number and increment
                } else {
                    vertexIndicesMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        //loop through every vertex in our grid
        for(int y = 0; y < borderedSize; y += meshSimplificationIncrement) {
            for(int x = 0; x < borderedSize; x += meshSimplificationIncrement) {
                
                int vertexIndex = vertexIndicesMap[x,y];

                //uvs are data that tell us were each vertex is in relation to the rest of the map, in the form of an (x,y) that is a percentage from 0 to 1
                Vector2 percent = new Vector2((x - meshSimplificationIncrement)/(float)meshSize, (y - meshSimplificationIncrement)/(float)meshSize);

                //we can use the uvs (percent from top left where our position is to calculate our vertex's position in space)
                //  We are also using an animation curve to transform our vertex height values into the values on the curve,
                //  e.g. making height values below .4 go to around 0 so our "water" is flat
                float height = heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                //add the vertex to the mesh data
                meshData.AddVertex(vertexPosition, percent, vertexIndex);
                

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
                //      triangle 1 (index, index + borderedSize + 1, index + borderedSize)
                //      triangle 2 (index + borderedSize + 1, index, index + 1)
                //
                //  these indices that we add to the triangle array can be in any order as long as they go "clock-wise" around the triangle (for unity at least)
                //
                //  e.g. a valid triangle would include indices 1, 2, and 5, or 2, 5, 1. Same indices in a different order makes the same triangle,
                //       as long as they are put in the array in a clock-wise order
                if((x < borderedSize - 1) && (y < borderedSize - 1)) {
                    int a = vertexIndicesMap[x,y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement,y];
                    int c = vertexIndicesMap[x,y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement,y + meshSimplificationIncrement];
                    
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

//a class for easily holding our mesh's data and creating a mesh from said data
public class MeshData {
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private int triangleIndex;
    private Vector3[] borderVertices;
    private int[] borderTriangles;
    private int borderTriangleIndex;

    //constructor
    public MeshData(int verticesPerLine) {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1)*(verticesPerLine - 1)*6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex) {

        //if the vertex index is negative that means it is in the border and we add it to the border vertices array
        if(vertexIndex < 0) {
            borderVertices[-vertexIndex - 1] = vertexPosition;

        //else if the vertex index is not negative we add it to the regular vertices array and add its uv to the array
        } else {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    //QOL method to aid in adding triangles to our triangles array
    public void AddTriangle(int a, int b, int c) {

        //if any of our points are negative that means they are in the border thus we add them to the border triangle array
        if(a < 0 || b < 0 || c < 0) {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;

        //if none of the points are negative then they are just a normal triangle and we add them to the mesh's triangle array
        } else {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    //calculate the surface normals for each vertex which is done by getting the average of every triangle's surface normal that each vertex is a part of
    //
    //          *---*  
    //         /1\2/3\
    //        *---@---*
    //
    // so like in the diagram above the vertex's (denoted with the @ symbol) normal will be the average of the three triangles' normal since it is a part of all three triangles
    private Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        //loop through all of the triangles
        for(int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            //get the vertices for the triangle
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            //calculate the triangle's normal from the vertices
            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            //add the triangle's normal to each of the vertices that makes up the triangle
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        //loop through all of the border triangles and add the normals to the vertices that are within the mesh
        for(int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            //get the vertices for the triangle
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            //calculate the triangle's normal from the vertices
            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            //add the triangle's normal to each of the vertices that makes up the triangle
            if(vertexIndexA >= 0) {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if(vertexIndexB >= 0) {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0) {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        //normalize the normals for each vertex
        for(int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    //calculate the normal of a triangle given its vertices' indices using the cross product of the vectors created from the 3 vertices
    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {

        //if the index is ever negative that means that the vertex is within the border vertices array so we must check so we can pull from there instead
        Vector3 pointA = (indexA < 0)? borderVertices[-indexA-1] : vertices[indexA];
        Vector3 pointB = (indexB < 0)? borderVertices[-indexB-1] : vertices[indexB];
        Vector3 pointC = (indexC < 0)? borderVertices[-indexC-1] : vertices[indexC];

        //calculate the vectors from the 3 vertex points
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        //return the cross product of the vectors which is the normal
        return Vector3.Cross(sideAB, sideAC).normalized;

    }

    //creates an actual mesh from our mesh data
    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
    
}
