using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{   
    private const float scale = 5f;
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public static float maxViewDistance = 450;
    //array to hold user inputted LODs and their respective distance thresholds
    public LODInfo[] detailLevels;
    public Transform viewer;

    public static Vector2 viewerPosition;
    private Vector2 previousViewerPosition;

    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDistance;
    public Material mapMaterial;

    //dictionary to save a terrain chunk and the coordinate its in (each chunk is 1 place on the grid, so middle chunk in 0,0 and chunk on the left is -1,0 etc.)
    private Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();

    //list of chunks we need to set not visible since theyre out of range
    private static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start() {

        mapGenerator = FindObjectOfType<MapGenerator>();
        //initialize number of chunks we can see with the set max view distance to calculate chunks we need to see obv
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }
    
    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        //only update the visible chunks if the viewer has moved a decent distance to that we dont have to update the chunks every frame needlessly
        if((previousViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            UpdateVisibleChunks();
            previousViewerPosition = viewerPosition;
        }
        
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

                //else if we haven't seen this chunk before add it to the dictionary of chunks we haven't seen
                } else {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        MapData mapData;
        bool mapDataRecieved;
        int previousLODIndex = -1;

        //initialize chunk data
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {

            this.detailLevels = detailLevels;

            //calculate the x,z position of the chunk
            position = coord * size;

            //creates bounding box centered in the middle of the chunk and then the size of the chunk
            chunkBounds = new Bounds(position, Vector2.one * size);
            //calculate the world position of the chunk
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            //create the chunk game object
            meshObject = new GameObject("Terrain Chunk");
            //give the chunk game object a mesh renderer and filter
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            //set the material for the chunk
            meshRenderer.material = material;
            //set the position of the chunk in the world
            meshObject.transform.position = positionV3 * scale;
            //set the scale of the chunk
            meshObject.transform.localScale = Vector3.one * scale;
            //set the parent of the chunk so they can all be under one parent
            meshObject.transform.parent = parent;
            //default set it to not visible
            SetVisible(false);

            //create all of the different meshes with the different level of details for this chunk
            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            //generate the map for the chunk which then generates the mesh for the chunk then sets the mesh for the chunk
            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        //once the map data has been generated set the map data to this chunk, designate that the data has been recieved, and generate and set the texture based on the 
        //  map data that was generated
        private void OnMapDataRecieved(MapData mapData) {
            this.mapData = mapData;
            mapDataRecieved = true;
            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }

        //finds the point on the chunk's perimeter that is closest to the player and if the distance is greater than the max view distance then it will disable it
        public void UpdateTerrainChunk() {
            if(!mapDataRecieved) {return;}

            //returns the smallest sqr distance between the given position and the bounding box
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            //if the chunk is visible set the mesh to the correct LOD mesh that corresponds to its distance away from the viewer
            if(visible) {
                int lodIndex = 0;
                //loop through all of the distance thresholds to check which LOD corresponds to its distance from the viewer
                for(int i = 0; i < detailLevels.Length - 1; i++) {
                    if(viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
                        lodIndex = i+1;
                    } else {
                        break;
                    }
                }

                //if the chunk is in a different LOD threshold than on the previous frame, change the mesh to match which LOD it should be
                if(lodIndex != previousLODIndex) {

                    //grab the mesh from the array of different LOD meshes
                    LODMesh lodMesh = lodMeshes[lodIndex];

                    //if the mesh for the LOD we're in has been created then set the chunks mesh to the LOD mesh
                    if(lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;

                    //if the mesh hasnt been created yet (and the mesh hasnt even been requested yet) request that the mesh be created
                    //  (if it has been requested but not created then we will wait for a later frame to update the LOD)
                    } else if(!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                //add this chunk to the visible last update list since it was indeed visible so that we can set it invisible on the next frame
                terrainChunksVisibleLastUpdate.Add(this);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }

    //class used to hold the meshes of differed LODs
    private class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        //once the mesh data had been generated create the mesh from it
        private void OnMeshDataRecieved(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        //request that mesh data be created from the given map data with the correct LOD
        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    //struct to hold data relating to the Levels of Detail and their influence based on the distance from the viewer
    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
