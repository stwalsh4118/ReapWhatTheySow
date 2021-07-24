using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public InfiniteTerrain terrainGenerator;
    public MapGenerator mapGenerator;

    public enum DeformationShape {
        CIRCLE,
        SQUARE
    }

    public List<int> GetListOfVertexIndicesAroundShapeAtPoint(Vector3 worldPoint, DeformationShape shape, int radius) {
        List<int> vertexIndices = new List<int>();
        Vector2 coord =new Vector2(Mathf.RoundToInt(worldPoint.x / (mapGenerator.terrainData.uniformScale * mapGenerator.mapChunkSize) ), Mathf.RoundToInt(worldPoint.z / (mapGenerator.terrainData.uniformScale * mapGenerator.mapChunkSize) ));
        int xCenter = Mathf.RoundToInt((worldPoint.x / mapGenerator.terrainData.uniformScale) - (coord.x * mapGenerator.mapChunkSize));
        int yCenter = Mathf.RoundToInt((worldPoint.z / mapGenerator.terrainData.uniformScale) - (coord.y * mapGenerator.mapChunkSize));
        xCenter += Mathf.RoundToInt(mapGenerator.mapChunkSize / 2);
        yCenter += Mathf.RoundToInt(mapGenerator.mapChunkSize / 2);

        //vertexIndices.Add(FindVertexIndex(new Vector2(yCenter, xCenter)));
        //Debug.Log(vertexIndices[0]);
        Debug.Log(FindVertexIndex(new Vector2(xCenter,yCenter)));
        vertexIndices.Add(FindVertexIndex(new Vector2(xCenter, yCenter)));
        if(shape == DeformationShape.CIRCLE) {
            for (int x = xCenter - radius ; x <= xCenter; x++){
                for (int y = yCenter - radius ; y <= yCenter; y++){
                    // we don't have to take the square root, it's slow
                    if ((x - xCenter)*(x - xCenter) + (y - yCenter)*(y - yCenter) <= radius*radius) {
                        int xSym = xCenter - (x - xCenter);
                        int ySym = yCenter - (y - yCenter);
                        // (x, y), (x, ySym), (xSym , y), (xSym, ySym) are in the circle
                        vertexIndices.Add(FindVertexIndex(new Vector2(x,y)));
                        vertexIndices.Add(FindVertexIndex(new Vector2(x,ySym)));
                        vertexIndices.Add(FindVertexIndex(new Vector2(xSym,y)));
                        vertexIndices.Add(FindVertexIndex(new Vector2(x,ySym)));
                        // Debug.Log(new Vector2(x,y));
                        // Debug.Log(new Vector2(x,ySym));
                        // Debug.Log(new Vector2(xSym,y));
                        // Debug.Log(new Vector2(xSym,ySym ));
                    }
                }
            }
        } else if(shape == DeformationShape.SQUARE) {
            for(int x = -radius; x <= radius; x++) {
                for(int y = -radius; y <= radius; y++) {
                    vertexIndices.Add(FindVertexIndex(new Vector3(xCenter + x, yCenter + y)));
                }
            }
        }
           
        return vertexIndices;
    }

    public Mesh DeformMesh(List<int> vertexIndices, Mesh mesh, float height) {
        Vector3[] vertices = mesh.vertices;
        for(int i = 0; i < vertexIndices.Count; i++) {
            vertices[vertexIndices[i]] += new Vector3(0, height, 0);
        }
        mesh.vertices = vertices;
        return mesh;
    }

    public int FindVertexIndex(Vector2 point) {
        int vertexIndex = (Mathf.RoundToInt(point.x) + ((mapGenerator.mapChunkSize - 1) - Mathf.RoundToInt(point.y)) * mapGenerator.mapChunkSize);
        if(vertexIndex < 0 || vertexIndex >= 57121) {
            return vertexIndex < 0 ? 0 : 57120;
        }
        return vertexIndex;
    }
}
