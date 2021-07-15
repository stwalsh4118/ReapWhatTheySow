using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{   
    public const float maxViewDistance = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunksVisibleInViewDistance;

    //dictionary to save a terrain chunk and the coordinate its in (each chunk is 1 place on the grid, so middle chunk in 0,0 and chunk on the left is -1,0 etc.)
    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();

    //list of chunks we need to set not visible since theyre out of range
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start() {

        //initialize number of chunks we can see with the set max view distance to calculate chunks we need to see obv
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }
    
    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks() {

        //set all the chunks we saw last update to not visible since there may be some that we shouldn't be seeing anymore
        foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate) {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        //get the chunk coordinates like described above
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        //loop through all the chunks we should be able to see
        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for(int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {

                //calculate the chunks coord from where the viewer is currently
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                //if we've seen this chunk before update it appropriately
                if(terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();

                    //if we can see the chunk this frame, add it to the viewed chunks list to be disabled next frame
                    if(terrainChunkDict[viewedChunkCoord].IsVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                    }

                //else if we haven't seen this chunk before add it to the dictionary of chunks we haven't seen
                } else {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    //class created to hold the data for our chunks
    public class TerrainChunk {

        //holds the mesh for the chunk that will be rendered to the screen, the position of said chunk, and the perimeter bounds of the chunk so we know if its in render distance
        GameObject meshObject;
        Vector2 position;
        Bounds chunkBounds;

        //initialize chunk data
        public TerrainChunk(Vector2 coord, int size, Transform parent) {

            position = coord * size;
            chunkBounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        //finds the point on the chunk's perimeter that is closest to the player and if the distance is greater than the max view distance then it will disable it
        public void UpdateTerrainChunk() {
            //returns the smallest sqr distance between the given position and the bounding box
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}
